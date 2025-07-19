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
    private Texture2D _star;
    private Texture2D _shot;
    private double _rotation;
    private bool _starfieldInitialized = false;

    private ushort _previousAnimationIndex;
    private ushort _currentAnimationIndex;

    private Rectangle[] _sourceRectangles;
    private int[] _translations;
    private int[] _starX;
    private int[] _starY;
    private float _shotX;
    private float _shotY;
    private float _shotXDelta;
    private float _shotYDelta;
    private bool _shotFired = false;
    private const int SpriteDimension = 61;
    private const int CircleDegrees = 360;
    private const int AngleIncrement = 15;
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
        _starX = new int[150];
        _starY = new int[150];
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Console.WriteLine(GraphicsDevice.Viewport.Width);
        Console.WriteLine(GraphicsDevice.Viewport.Height);


        _image = Content.Load<Texture2D>("ship_red");
        _timer = 0;
        _threshold = 250;

        int sourceRectangleCount = 0;
        _sourceRectangles = new Rectangle[24];
        _translations = new int[24];
        _previousAnimationIndex = 0;

        for (int y = 0; y < (SpriteDimension * 4); y += SpriteDimension)
        {
            for (int x = 0; x < (SpriteDimension * 6); x += SpriteDimension)
            {
                _sourceRectangles[sourceRectangleCount] = new Rectangle(x, y, SpriteDimension, SpriteDimension);

                if (sourceRectangleCount < 6)
                {
                    _translations[sourceRectangleCount] = 90 - (AngleIncrement * sourceRectangleCount);
                }
                else
                {
                    _translations[sourceRectangleCount] = CircleDegrees - (sourceRectangleCount - 6) * AngleIncrement;
                }
                sourceRectangleCount++;

            }
        }

        _star = new Texture2D(GraphicsDevice, 1, 1);
        _star.SetData(new[] { Color.White });
        _shot = new Texture2D(GraphicsDevice, 8, 8);
        _shot.SetData(new[] { Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red,
                             Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red,
                             Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red,
                             Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red,
                             Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red,
                             Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red,
                             Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red,Color.Red ,Color.Red,
                             Color.Red ,Color.Red ,Color.Red ,Color.Red ,Color.Red ,Color.Red ,Color.Red ,Color.Red});
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
            // Console.WriteLine(_translations[_currentAnimationIndex]);
            double radians = (Math.PI / 180.0) * _translations[_currentAnimationIndex];
            float _posXDelta = (float)Math.Cos(radians);
            float _posYDelta = (float)Math.Sin(radians);
            _posX += _posXDelta;
            _posY += (_posYDelta * -1);
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            if (_shotFired) return;
            
            _shotFired = true;
            _rotation = (Math.PI / 180.0) * _translations[_currentAnimationIndex];
            _shotX = _posX + SpriteDimension / 2;
            _shotY = _posY + SpriteDimension / 2;
            
            Console.WriteLine($"Shot fired at angle {_translations[_currentAnimationIndex]} degrees");
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
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();

        

        if (_shotFired)
        {
            _shotXDelta = (float)Math.Cos(_rotation) * 10;
            _shotYDelta = (float)Math.Sin(_rotation) * -10;
            _shotX += _shotXDelta;
            _shotY += _shotYDelta;
            _spriteBatch.Draw(_shot, new Vector2(_shotX, _shotY), Color.Red);

            if (_shotX < 0 || _shotX > GraphicsDevice.Viewport.Width || _shotY < 0 || _shotY > GraphicsDevice.Viewport.Height)
            {
                _shotFired = false;
                _shotX = 0;
                _shotY = 0;
            }
        }

        if (!_starfieldInitialized)
        {

            for (int i = 0; i < 150; i++)
            {
                int x = (new Random()).Next(0, GraphicsDevice.Viewport.Width);
                int y = (new Random()).Next(0, GraphicsDevice.Viewport.Height);
                _starX[i] = x;
                _starY[i] = y;
                _spriteBatch.Draw(_star, new Vector2(x, y), Color.White);
            }
            _starfieldInitialized = true;
        }
        else
        {
            for (int i = 0; i < 150; i++)
            {
                _spriteBatch.Draw(_star, new Vector2(_starX[i], _starY[i]), Color.White);
            }
        }

        Rectangle sourceRectangle = new Rectangle(0, 0, 61, 61);

        // _spriteBatch.Draw(_image, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
        // _spriteBatch.Draw(_image, new Vector2(300, 100), sourceRectangle, Color.White);
        _spriteBatch.Draw(_image, new Vector2(_posX, _posY), _sourceRectangles[_currentAnimationIndex], Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
        Console.WriteLine($"Drawing ship at angle {_translations[_currentAnimationIndex]} degrees at position {_posX},{_posY} with source rectangle {_sourceRectangles[_currentAnimationIndex]}");
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
