using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Prototypes.Icebox;

public class Icebox : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 19;
        Item.height = 24;

        Item.noMelee = true;

        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.autoReuse = true;
        Item.channel = true;
        Item.noUseGraphic = false;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item24;
            
        Item.shoot = ProjectileType<IceboxManager>();
        Item.shootSpeed = 2;
    }
}

public class IceboxManager : ModProjectile
{
    public override string Texture => "Prototypes/Icebox/Icebox";

    public List<TilePosition> managedTiles = new List<TilePosition>();
    
    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.timeLeft = 3600;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 127;
    }

    private void AddTile(int x, int y)
    {
        TilePosition toAdd = new TilePosition(x, y);
        foreach (TilePosition tile in managedTiles)
        {
            if (tile.x == x && tile.y == y) return;
        }
        
        managedTiles.Add(toAdd);
    }

    public override bool ShouldUpdatePosition()
    {
        return false;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        
        int x = (int)(Main.MouseWorld.X / 16);
        int y = (int)(Main.MouseWorld.Y / 16);
        if (!Main.tile[x, y].HasTile) AddTile(x, y);
        if (!player.channel || !player.active || player.dead)
        {
            Projectile.Kill();
            return;
        }
    }

    public override void OnKill(int timeLeft)
    {
        bool mute = false;
        foreach (TilePosition tile in managedTiles)
        {
            WorldGen.PlaceTile(tile.x, tile.y, TileID.MagicalIceBlock, mute);
            GetInstance<IceBreaker>().iceBreaking.TryAdd(new TilePosition(tile.x, tile.y), 180 * -0.0001f);
            mute = true;    
        }

        base.OnKill(timeLeft);
    }

    private void AddBorderPoints(Vector2 point1, Vector2 point2, int numInterps, ref List<Vector2> list)
    {
        list.Add(point1);
        for (float i = 0; i < 1f; i += 1f / numInterps)
        {
            list.Add(Vector2.Lerp(point1, point2, i));
        }
        list.Add(point2);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.MagicPixel.Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < managedTiles.Count - 1; i++)
        {
            for (float j = 0; j < 1f; j += 0.05f)
            {
                points.Add(Vector2.Lerp(new Vector2(managedTiles[i].x * 16 + 8, managedTiles[i].y * 16 + 8),
                    new Vector2(managedTiles[i + 1].x * 16 + 8, managedTiles[i + 1].y * 16 + 8), j));
            }
        }

        if (points.Count > 1) DrawLine(points, new Color(180, 255, 255));

        return false;
    }
    
    private void DrawLine(List<Vector2> list, Color color)
    {
        Texture2D texture = TextureAssets.FishingLine.Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = new Vector2(frame.Width / 2, 2);

        Vector2 pos = list[0];
        for (int i = 0; i < list.Count - 1; i++)
        {
            Vector2 element = list[i];
            Vector2 diff = Vector2.Zero;
            diff = list[i + 1] - element;

            float rotation = diff.ToRotation() - MathHelper.PiOver2;
            Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

            pos += diff;
        }
    }
    
    private Color ColorLerpCycle(int i, int size, Color[] colors)
    {
        float fraction = (float)i / size;
        float convertedFraction = fraction * colors.Length;
        Color lowColor = colors[(int)convertedFraction];
        Color highColor = colors[0];
        if ((int)convertedFraction + 1 < colors.Length) highColor = colors[(int)convertedFraction + 1];
        return Color.Lerp(lowColor, highColor, convertedFraction - (int)convertedFraction);
    }
}

public struct TilePosition
{
    public int x;
    public int y;
    public TilePosition(int x, int y) { this.x = x; this.y = y; }
};

public class IceBreaker : ModSystem
{
    public Dictionary<TilePosition, float> iceBreaking = new Dictionary<TilePosition, float>();

    public override void PostUpdateWorld()
    {
        foreach (KeyValuePair<TilePosition, float> ice in iceBreaking)
        {
            if (Main.tile[ice.Key.x, ice.Key.y].TileType != TileID.MagicalIceBlock)
            {
                iceBreaking.Remove(ice.Key);
                continue;
            }
            if (Main.rand.NextFloat() < ice.Value)
            {
                WorldGen.KillTile(ice.Key.x, ice.Key.y);
                iceBreaking.Remove(ice.Key);
            }
            else iceBreaking[ice.Key] += 0.0001f;
        }
    }
}