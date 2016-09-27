using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BulletMLLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletMLSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class Game1 : Microsoft.Xna.Framework.Game
    {
        static public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D texture;
        Texture2D playerTexture;

        static public Myship myship;
        static public BulletMLParser parser; //BulletMLを解析し、解析結果を保持するクラス。XMLごとに必要です。
        static public Random rand = new Random();
        Mover mover;

        private KeyboardState previousKeyboardState;

        private List<String> patternsFilename = new List<string>();
        private int patternIndex = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            //自機初期化
            myship = new Myship();
            myship.Init();

            previousKeyboardState = Keyboard.GetState();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            texture = Content.Load<Texture2D>("Sprites\\bullets");
            playerTexture = Content.Load<Texture2D>("Sprites\\player");

            foreach (var source in Directory.GetFiles("Content\\xml\\Tests", "*.xml"))
            {
                patternsFilename.Add(source);
            }

            ParseNewPattern();

            AddBullet();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.PageUp) && previousKeyboardState.IsKeyUp(Keys.PageUp))
            {
                patternIndex = (patternIndex + 1) % (patternsFilename.Count - 1);
                ParseNewPattern();
                AddBullet();
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.PageDown) && previousKeyboardState.IsKeyUp(Keys.PageDown))
            {
                patternIndex = (patternIndex - 1) < 0 ? patternsFilename.Count - 1 : patternIndex - 1;
                ParseNewPattern();
                AddBullet();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && previousKeyboardState.IsKeyUp(Keys.LeftControl))
                AddBullet();

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                AddBullet();

            //すべてのMoverを行動させる
            MoverManager.Update();
            //使わなくなったMoverを解放
            MoverManager.FreeMovers();
            // 自機を更新
            myship.Update();

            previousKeyboardState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //敵や弾を描画
            spriteBatch.Begin();

            foreach (Mover mover in MoverManager.movers)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int) mover.pos.X, (int) mover.pos.Y, texture.Width, texture.Height), null,
                    Color.White, mover.Dir,
                    new Vector2(texture.Width/2f, texture.Height/2f), SpriteEffects.None, 0f
                );
            }

            spriteBatch.Draw(playerTexture, myship.pos, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void ParseNewPattern()
        {
            parser = new BulletMLParser();
            parser.ParseXML(patternsFilename[patternIndex]);
            BulletMLManager.Init(new MyBulletFunctions());

            Debug.Print("Current pattern: " + patternsFilename[patternIndex]);
        }

        private void AddBullet()
        {
            MoverManager.FreeMovers();

            //敵を一つ画面中央に作成し、弾を吐くよう設定
            mover = MoverManager.CreateMover();
            mover.pos = new Vector2(graphics.PreferredBackBufferWidth / 2f, graphics.PreferredBackBufferHeight / 2f);
            mover.SetBullet(parser.tree); //BulletMLで動かすように設定
        }
    }
}
