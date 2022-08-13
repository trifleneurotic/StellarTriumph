using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace StellarTriumph;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _image;

    private ushort _previousAnimationIndex;
    private ushort _currentAnimationIndex;

    private Rectangle[] _sourceRectangles;

    private const int SpriteDimension = 61;
    private float _timer;
    private int _threshold;

    private float _posX = 100.0f;
    private float _posY = 100.0f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _image = Content.Load<Texture2D>("ship_red");
        _timer = 0;
        _threshold = 250;

        int sourceRectangleCount = 0;
        _sourceRectangles = new Rectangle[24];
        _previousAnimationIndex = 0;

        for(int y = 0; y < (SpriteDimension * 4); y += SpriteDimension)
        {
            for(int x = 0; x < (SpriteDimension * 6); x += SpriteDimension)
            {
                _sourceRectangles[sourceRectangleCount] = new Rectangle(x, y, SpriteDimension, SpriteDimension);
                sourceRectangleCount++;
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            if (_previousAnimationIndex == 0)
            {
                _currentAnimationIndex = 23;
            }
            else
            {
                _currentAnimationIndex = (ushort)((_previousAnimationIndex - 1) % 23);  
            }
            _previousAnimationIndex = _currentAnimationIndex;  
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            _currentAnimationIndex = (ushort)((_previousAnimationIndex + 1) % 23);
            _previousAnimationIndex = _currentAnimationIndex;
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            double radians = (Math.PI / 180) * (360 / (_currentAnimationIndex + 1));
            float _posXDelta = (float)Math.Cos(radians);
            float _posYDelta = (float)Math.Sin(radians);
            _posX += _posXDelta;
            _posY += _posYDelta;
        }
        else
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        /* if (_timer > _threshold)
        {
            _currentAnimationIndex = (ushort)((_previousAnimationIndex + 1) % 23);
            _previousAnimationIndex = _currentAnimationIndex;
            _timer = 0;
        }
        else
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        }
        */

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        Rectangle sourceRectangle = new Rectangle(0, 0, 61, 61);

        // _spriteBatch.Draw(_image, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
        // _spriteBatch.Draw(_image, new Vector2(300, 100), sourceRectangle, Color.White);
        _spriteBatch.Draw(_image, new Vector2(_posX, _posY), _sourceRectangles[_currentAnimationIndex], Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
