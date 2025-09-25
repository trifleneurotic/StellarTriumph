using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace StellarTriumph;


public class STMain : Game
{

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _redShip;
    private Texture2D _blueShip;
    private Texture2D _star;
    private Texture2D _shot;
    private Texture2D _monolithLeft;
    private Texture2D _monolithRight;
    private Texture2D _monolithTop;
    private Texture2D _monolithBottom;
    private double _rotation;
    private bool _starfieldInitialized = false;
    private bool _monolithHit = false;

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
    private int _xCoefficient = 1;
    private int _yCoefficient = 1;
    private bool _shotFired = false;
    private const int SpriteDimension = 61;
    private const int CircleDegrees = 360;
    private const int AngleIncrement = 15;
    private float _timer;
    private int _threshold;

    private float _redPosX = 100.0f;
    private float _redPosY = 100.0f;
    private float _bluePosX = 500.0f;
    private float _bluePosY = 100.0f;

    public STMain()
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


        _redShip = Content.Load<Texture2D>("ship_red");
        _blueShip = Content.Load<Texture2D>("ship_blue");
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
        _monolithTop = new Texture2D(GraphicsDevice, 160, 2);
        _monolithBottom = new Texture2D(GraphicsDevice, 160, 2);
        _monolithLeft = new Texture2D(GraphicsDevice, 2, 16);
        _monolithRight = new Texture2D(GraphicsDevice, 2, 16);
        Color[] monolithColorData = new Color[160 * 2];
        for (int i = 0; i < monolithColorData.Length; i++)
        {
            monolithColorData[i] = Color.Gray;
        }
        _monolithTop.SetData(monolithColorData);
        _monolithBottom.SetData(monolithColorData);
        Color[] monolithSideColorData = new Color[2 * 16];
        for (int i = 0; i < monolithSideColorData.Length; i++)
        {
            monolithSideColorData[i] = Color.Gray;
        }
        _monolithLeft.SetData(monolithSideColorData);
        _monolithRight.SetData(monolithSideColorData);
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
            _redPosX += _posXDelta;
            _redPosY += (_posYDelta * -1);
        }
        else
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            if (_shotFired) return;
            
            _shotFired = true;
            _rotation = (Math.PI / 180.0) * _translations[_currentAnimationIndex];
            _shotX = _redPosX + SpriteDimension / 2;
            _shotY = _redPosY + SpriteDimension / 2;
            
            Console.WriteLine($"Shot fired at angle {_translations[_currentAnimationIndex]} degrees");
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


        _spriteBatch.Draw(_monolithTop, new Rectangle(300, 300, 280, 1), Color.Gray);
        _spriteBatch.Draw(_monolithBottom, new Rectangle(300, 300 + 24, 280, 1), Color.Gray);
        _spriteBatch.Draw(_monolithLeft, new Rectangle(300, 300, 1, 24), Color.Gray);
        _spriteBatch.Draw(_monolithRight, new Rectangle(300 + 280, 300, 1, 24), Color.Gray);
        
        if (_shotFired)
        {
            _shotXDelta = (float)Math.Cos(_rotation) * 10 * _xCoefficient;
            _shotYDelta = (float)Math.Sin(_rotation) * -10 * _yCoefficient;

            _shotX += _shotXDelta;
            _shotY += _shotYDelta;
            
            Rectangle monolithRect = new Rectangle(300, 300, 280, 24);
            Rectangle shotRect = new Rectangle((int)_shotX, (int)_shotY, 8, 8);

            if (shotRect.Intersects(monolithRect))
            {
                if (shotRect.Intersects(new Rectangle(300, 300, 280, 1)) || shotRect.Intersects(new Rectangle(300, 300 + 24, 280, 1)))
                {
                    _yCoefficient *= -1;
                }
                if (shotRect.Intersects(new Rectangle(300, 300, 1, 24)) || shotRect.Intersects(new Rectangle(300 + 280, 300, 1, 24)))
                {
                    _xCoefficient *= -1;
                }
            }

            _spriteBatch.Draw(_shot, new Vector2(_shotX, _shotY), Color.Red);

            if (_shotX < 0 || _shotX > GraphicsDevice.Viewport.Width || _shotY < 0 || _shotY > GraphicsDevice.Viewport.Height)
            {
                _shotFired = false;
                _monolithHit = false;
                _shotX = 0;
                _shotY = 0;
                _xCoefficient = 1;
                _yCoefficient = 1;
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
        _spriteBatch.Draw(_redShip, new Vector2(_redPosX, _redPosY), _sourceRectangles[_currentAnimationIndex], Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
        _spriteBatch.Draw(_blueShip, new Vector2(_bluePosX, _bluePosY), _sourceRectangles[0], Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
        Console.WriteLine($"Drawing ship at angle {_translations[_currentAnimationIndex]} degrees at position {_redPosX},{_redPosY} with source rectangle {_sourceRectangles[_currentAnimationIndex]}");
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
