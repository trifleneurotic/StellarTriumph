using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGame.Extended.Collisions;
using MonoGame.Extended;
using System.Xml;
using Myra;
using Myra.Graphics2D.UI;


namespace StellarTriumph;

public class STMain : Game
{
    public enum GameState
    {
        MainMenu,
        Gameplay,
        Paused,
        GameOver
    }
    private GameState _currentGameState = GameState.MainMenu;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _redShip;
    private Texture2D _explosion;
    private Texture2D _star;

    private Texture2D _shot;
    private Texture2D _shotBlue;
    private Texture2D _blueShip;
    private Texture2D _monolithLeft;
    private Texture2D _monolithRight;
    private Texture2D _monolithTop;
    private Texture2D _monolithBottom;
    private double _rotation;
    private double _rotationBlue;
    private bool _starfieldInitialized = false;
    private bool _monolithHit = false;
    private bool _inExplosion = false;
    private bool _inExplosionRed = false;

    private ushort _previousAnimationIndex;
    private ushort _currentAnimationIndex;

    private ushort _previousAnimationIndexBlue;
    private ushort _currentAnimationIndexBlue;

    private Rectangle[] _sourceRectangles;
    private int[] _translations;
    private int[] _starX;
    private int[] _starY;
    private float _shotX;
    private float _shotY;
    private float _shotXBlue;
    private float _shotYBlue;
    private float _shotXDelta;
    private float _shotYDelta;

    private float _shotXDeltaBlue;
    private float _shotYDeltaBlue;
    private int _xCoefficient = 1;
    private int _yCoefficient = 1;

    private int _xCoefficientBlue = 1;
    private int _yCoefficientBlue = 1;
    private bool _shotFired = false;
    private bool _shotFiredBlue = false;
    private const int SpriteDimension = 61;
    private const int CircleDegrees = 360;
    private const int AngleIncrement = 15;
    private float _timer;
    private int _threshold;
    private int _fuel = 1024;
    private int _fuelBlue = 1024;
    private int _shots = 16;
    private int _shotsBlue = 16;
    private float _inertial = 0.0F;
    private float _inertialBlue = 0.0F;
    private float _inertialDeltaX;
    private float _inertialDeltaY;

    private float _inertialDeltaXBlue;
    private float _inertialDeltaYBlue;

    private float _redPosX = 100.0f;
    private float _redPosY = 100.0f;
    private float _bluePosX = 500.0f;
    private float _bluePosY = 100.0f;

    int _currentExplosionFrameIndex = 0;
    float _timePerExplosionFrame = 0.1f; // seconds
    float _explosionElapsedTime = 0f;

    Rectangle[] animationFrames = new Rectangle[16]; // Or a List<Rectangle>

    // The Sprite Font reference to draw with
    SpriteFont font1;
    private Desktop _desktop;

    // The position to draw the text
    Vector2 fontPos;
    Vector2 fontPos2;

    Vector2 fontPos3;
    Vector2 fontPos4;
      

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
        Window.Title = "Stellar Triumph";
    }

    protected override void LoadContent()
    {
        MyraEnvironment.Game = this;
        var grid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 8
        };

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var helloWorld = new Label
        {
            Id = "label",
            Text = "Stellar Triumph"
        };
        grid.Widgets.Add(helloWorld);

        var toggle = new Label
        {
            Id = "toggle",
            Text = "Toggles"
        };
        Grid.SetColumn(toggle, 0);
        Grid.SetRow(toggle, 2);       
        grid.Widgets.Add(toggle);

        var monolithToggle = new ToggleButton
        {
            Content = new Label
	{
		HorizontalAlignment = HorizontalAlignment.Center,
		VerticalAlignment = VerticalAlignment.Center,
		Text = "Monolith"
	}
        };
        Grid.SetColumn(monolithToggle, 1);
        Grid.SetRow(monolithToggle, 2);
        grid.Widgets.Add(monolithToggle);

          var inertiaToggle = new ToggleButton
        {
            Content = new Label
	{
		HorizontalAlignment = HorizontalAlignment.Center,
		VerticalAlignment = VerticalAlignment.Center,
		Text = "Inertia"
	}
        };
        Grid.SetColumn(inertiaToggle, 2);
        Grid.SetRow(inertiaToggle, 2);
        grid.Widgets.Add(inertiaToggle);

        var button = new Button
        {
            Content = new Label
            {
                Text = "Start Game"
            }
        };
        Grid.SetColumn(button, 0);
        Grid.SetRow(button, 1);

        button.Click += (s, a) =>
        {
            _currentGameState = GameState.Gameplay;
        };

        var button2 = new Button
        {
            Content = new Label
            {
                Text = "Quit"
            }
        };
        Grid.SetColumn(button2, 1);
        Grid.SetRow(button2, 1);

        button2.Click += (s, a) =>
        {
            Exit();
        };

        grid.Widgets.Add(button);
        grid.Widgets.Add(button2);
        
        _desktop = new Desktop();
        _desktop.Root = grid;

        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Console.WriteLine(GraphicsDevice.Viewport.Width);
        Console.WriteLine(GraphicsDevice.Viewport.Height);

        _redShip = Content.Load<Texture2D>("ship_red");
        _blueShip = Content.Load<Texture2D>("ship_blue");
        _explosion = Content.Load<Texture2D>("Explosion");
        _timer = 0;
        _threshold = 250;

        int sourceRectangleCount = 0;
        _sourceRectangles = new Rectangle[24];
        _translations = new int[24];
        _previousAnimationIndex = 0;

        animationFrames[0] = new Rectangle(0, 0, 64, 64);
        animationFrames[1] = new Rectangle(64, 0, 64, 64);
        animationFrames[2] = new Rectangle(128, 0, 64, 64);
        animationFrames[3] = new Rectangle(192, 0, 64, 64);
        animationFrames[4] = new Rectangle(0, 64, 64, 64);
        animationFrames[5] = new Rectangle(64, 64, 64, 64);
        animationFrames[6] = new Rectangle(128, 64, 64, 64);
        animationFrames[7] = new Rectangle(192, 64, 64, 64);
        animationFrames[8] = new Rectangle(0, 128, 64, 64);
        animationFrames[9] = new Rectangle(64, 128, 64, 64);
        animationFrames[10] = new Rectangle(128, 128, 64, 64);
        animationFrames[11] = new Rectangle(192, 128, 64, 64);
        animationFrames[12] = new Rectangle(0, 192, 64, 64);
        animationFrames[13] = new Rectangle(64, 192, 64, 64);
        animationFrames[14] = new Rectangle(128, 192, 64, 64);
        animationFrames[15] = new Rectangle(192, 192, 64, 64);

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
        _shotBlue = new Texture2D(GraphicsDevice, 8, 8);
        _shotBlue.SetData(new[] { Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red, Color.Red,
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

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font1 = Content.Load<SpriteFont>("MyMenuFont");
        Viewport viewport = _graphics.GraphicsDevice.Viewport;

        // TODO: Load your game content here            
        fontPos = new Vector2(30, 30);
        fontPos2 = new Vector2(30, 60);
        fontPos3 = new Vector2(300, 30);
        fontPos4 = new Vector2(300, 60);
    }


    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            _currentGameState = GameState.MainMenu;
        switch (_currentGameState)
        {
            case GameState.MainMenu:
                base.Update(gameTime);
                return;
            case GameState.Gameplay:
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    //if (gameTime.TotalGameTime.Milliseconds % 50 == 0)
                    //{

                        if (_previousAnimationIndex == 0)
                        {
                            _currentAnimationIndex = 23;
                        }
                        else
                        {
                            _currentAnimationIndex = (ushort)((_previousAnimationIndex - 1) % 23);
                        }
                        _previousAnimationIndex = _currentAnimationIndex;
                        if (Keyboard.GetState().IsKeyDown(Keys.Up))
                        {
                            _inertialDeltaX *= _inertial;
                            _inertialDeltaY *= _inertial;
                            _redPosX += _inertialDeltaX * 4;
                            _redPosY += (_inertialDeltaY * -1) * 4;                    
                        }
                    //}
                }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    {
                    //if (gameTime.TotalGameTime.Milliseconds % 50 == 0)
                    //{
                        _currentAnimationIndex = (ushort)((_previousAnimationIndex + 1) % 23);
                        _previousAnimationIndex = _currentAnimationIndex;
                            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                        {
                            _inertialDeltaX *= _inertial;
                            _inertialDeltaY *= _inertial;
                            _redPosX += _inertialDeltaX * 4;
                            _redPosY += (_inertialDeltaY * -1) * 4;                    
                        }
                        //}
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    {
                        if (_fuel > 0)
                        {
                            // Console.WriteLine(_translations[_currentAnimationIndex]);
                            double radians = (Math.PI / 180.0) * _translations[_currentAnimationIndex];
                            float _posXDelta = (float)Math.Cos(radians);
                            float _posYDelta = (float)Math.Sin(radians);
                            _redPosX += _posXDelta * 2;
                            _redPosY += (_posYDelta * -1) * 2;
                            _inertialDeltaX = _posXDelta;
                            _inertialDeltaY = _posYDelta;
                            _inertial = 1.0F;

                            if (gameTime.ElapsedGameTime.Seconds % 8 == 0)
                            {
                                _fuel -= 1;
                            }
                        }
                    }
                    else
                    {
                        _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }

                if (Keyboard.GetState().IsKeyUp(Keys.Up) && _inertial > 0)
                {
                    //Console.WriteLine(_inertial);
                    //_inertial -= 0.001F;
                    _inertialDeltaX *= _inertial;
                    _inertialDeltaY *= _inertial;
                    _redPosX += _inertialDeltaX * 2;
                    _redPosY += (_inertialDeltaY * -1) * 2;
                }



                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    if (_shotFired || _shots == 0) return;

                    _shotFired = true;
                    _rotation = (Math.PI / 180.0) * _translations[_currentAnimationIndex];
                    _shotX = _redPosX + SpriteDimension / 2;
                    _shotY = _redPosY + SpriteDimension / 2;

                    Console.WriteLine($"Shot fired at angle {_translations[_currentAnimationIndex]} degrees");
                    _shots--;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    if (gameTime.TotalGameTime.Milliseconds % 50 == 0)
                    {
                        if (_previousAnimationIndexBlue == 0)
                        {
                            _currentAnimationIndexBlue = 23;
                        }
                        else
                        {
                            _currentAnimationIndexBlue = (ushort)((_previousAnimationIndexBlue - 1) % 23);
                        }
                        _previousAnimationIndexBlue = _currentAnimationIndexBlue;
                        if (Keyboard.GetState().IsKeyDown(Keys.W))
                        {
                            _inertialDeltaXBlue *= _inertialBlue;
                            _inertialDeltaYBlue *= _inertialBlue;
                            _bluePosX += _inertialDeltaXBlue * 4;
                            _bluePosY += (_inertialDeltaYBlue * -1) * 4;                    
                        }
                    }
                }
                    else if (Keyboard.GetState().IsKeyDown(Keys.D))
                    {
                    if (gameTime.TotalGameTime.Milliseconds % 50 == 0)
                    {
                        _currentAnimationIndexBlue = (ushort)((_previousAnimationIndexBlue + 1) % 23);
                        _previousAnimationIndexBlue = _currentAnimationIndexBlue;
                        if (Keyboard.GetState().IsKeyDown(Keys.W))
                        {
                            _inertialDeltaXBlue *= _inertialBlue;
                            _inertialDeltaYBlue *= _inertialBlue;
                            _bluePosX += _inertialDeltaXBlue * 4;
                            _bluePosY += (_inertialDeltaYBlue * -1) * 4;                    
                        }
                    }
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.W))
                    {
                        if (_fuelBlue > 0)
                        {
                            // Console.WriteLine(_translations[_currentAnimationIndex]);
                            double radians = (Math.PI / 180.0) * _translations[_currentAnimationIndexBlue];
                            float _posXDelta = (float)Math.Cos(radians);
                            float _posYDelta = (float)Math.Sin(radians);
                            _bluePosX += _posXDelta * 2;
                            _bluePosY += (_posYDelta * -1) * 2;
                            _inertialDeltaXBlue = _posXDelta;
                            _inertialDeltaYBlue = _posYDelta;
                            _inertialBlue = 1.0F;

                            if (gameTime.ElapsedGameTime.Seconds % 8 == 0)
                            {
                                _fuelBlue -= 1;
                            }
                        }
                    }
                    else
                    {
                        _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }

                if (Keyboard.GetState().IsKeyUp(Keys.W) && _inertialBlue > 0)
                {
                    //Console.WriteLine(_inertialBlue);
                    //_inertialBlue -= 0.001F;
                    _inertialDeltaXBlue *= _inertialBlue;
                    _inertialDeltaYBlue *= _inertialBlue;
                    _bluePosX += _inertialDeltaXBlue * 2;
                    _bluePosY += (_inertialDeltaYBlue * -1) * 2;
                }



                if (Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    if (_shotFiredBlue || _shotsBlue == 0) return;

                    _shotFiredBlue = true;
                    _rotationBlue = (Math.PI / 180.0) * _translations[_currentAnimationIndexBlue];
                    _shotXBlue = _bluePosX + SpriteDimension / 2;
                    _shotYBlue = _bluePosY + SpriteDimension / 2;

                    Console.WriteLine($"Blue shot fired at angle {_translations[_currentAnimationIndexBlue]} degrees");
                    _shotsBlue--;
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
                if (_inExplosion || _inExplosionRed)
                {
                    if (_currentExplosionFrameIndex > animationFrames.Length)
                    {
                        if (_inExplosion) _inExplosion = false;
                        if (_inExplosionRed) _inExplosionRed = false;
                        _currentExplosionFrameIndex = 0;
                    }
                    else
                    {
                        _explosionElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (_explosionElapsedTime >= _timePerExplosionFrame)
                        {
                            _currentExplosionFrameIndex++;

                            _explosionElapsedTime -= _timePerExplosionFrame;
                        }
                    }
                }

                base.Update(gameTime);
                break;
            case GameState.Paused:
                // Update paused state
                break;
            case GameState.GameOver:
                // Update game over state
                break;
        }

        
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

        switch (_currentGameState)
        {
            case GameState.MainMenu:
                _desktop.Render();
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            case GameState.Gameplay:
                string output = _fuel.ToString();
                string output2 = _shots.ToString();
                string output3 = _fuelBlue.ToString();
                string output4 = _shotsBlue.ToString();

                Vector2 FontOrigin = font1.MeasureString(output) / 2;
                _spriteBatch.DrawString(font1, output, fontPos, Color.LightGreen,
                0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);

                Vector2 FontOrigin2 = font1.MeasureString(output2) / 2;
                _spriteBatch.DrawString(font1, output2, fontPos2, Color.OrangeRed,
                0, FontOrigin2, 1.0f, SpriteEffects.None, 0.5f);

                Vector2 FontOrigin3 = font1.MeasureString(output3) / 2;
                _spriteBatch.DrawString(font1, output3, fontPos3, Color.LightGreen,
                0, FontOrigin3, 1.0f, SpriteEffects.None, 0.5f);

                Vector2 FontOrigin4 = font1.MeasureString(output4) / 2;
                _spriteBatch.DrawString(font1, output4, fontPos4, Color.OrangeRed,
                0, FontOrigin4, 1.0f, SpriteEffects.None, 0.5f);



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
                    Rectangle blueShipRect = new Rectangle((int)_bluePosX, (int)_bluePosY, SpriteDimension, SpriteDimension);

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

                    if (!shotRect.Intersects(blueShipRect))
                    {
                        _spriteBatch.Draw(_shot, new Vector2(_shotX, _shotY), Color.Red);
                        //_spriteBatch.Draw(_blueShip, new Vector2(_bluePosX, _bluePosY), _sourceRectangles[0], Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                    }
                    else
                    {
                        _shotFired = false;
                        _monolithHit = false;
                        _shotX = 0;
                        _shotY = 0;
                        _xCoefficient = 1;
                        _yCoefficient = 1;
                        _inExplosion = true;
                        // _blueShip.Dispose();
                    }

                    

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

                if (_shotFiredBlue)
                {
                    _shotXDeltaBlue = (float)Math.Cos(_rotationBlue) * 10 * _xCoefficientBlue;
                    _shotYDeltaBlue = (float)Math.Sin(_rotationBlue) * -10 * _yCoefficientBlue;

                    _shotXBlue += _shotXDeltaBlue;
                    _shotYBlue += _shotYDeltaBlue;

                    Rectangle monolithRect = new Rectangle(300, 300, 280, 24);
                    Rectangle shotRect = new Rectangle((int)_shotXBlue, (int)_shotYBlue, 8, 8);
                    Rectangle redShipRect = new Rectangle((int)_redPosX, (int)_redPosY, SpriteDimension, SpriteDimension);

                    if (shotRect.Intersects(monolithRect))
                    {
                        if (shotRect.Intersects(new Rectangle(300, 300, 280, 1)) || shotRect.Intersects(new Rectangle(300, 300 + 24, 280, 1)))
                        {
                            _yCoefficientBlue *= -1;
                        }
                        if (shotRect.Intersects(new Rectangle(300, 300, 1, 24)) || shotRect.Intersects(new Rectangle(300 + 280, 300, 1, 24)))
                        {
                            _xCoefficientBlue *= -1;
                        }
                    }

                    if (!shotRect.Intersects(redShipRect))
                    {
                        _spriteBatch.Draw(_shot, new Vector2(_shotXBlue, _shotYBlue), Color.Red);
                        //_spriteBatch.Draw(_redShip, new Vector2(_redPosX, _redPosY), _sourceRectangles[0], Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                    }
                    else
                    {
                        _shotFiredBlue = false;
                        _monolithHit = false;
                        _shotXBlue = 0;
                        _shotYBlue = 0;
                        _xCoefficientBlue = 1;
                        _yCoefficientBlue = 1;
                        _inExplosionRed = true;
                        // _blueShip.Dispose();
                    }

                    

                    if (_shotXBlue < 0 || _shotXBlue > GraphicsDevice.Viewport.Width || _shotYBlue < 0 || _shotYBlue > GraphicsDevice.Viewport.Height)
                    {
                        _shotFiredBlue = false;
                        _monolithHit = false;
                        _shotXBlue = 0;
                        _shotYBlue = 0;
                        _xCoefficientBlue = 1;
                        _yCoefficientBlue = 1;
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


                /* _redPosX = MathHelper.Clamp(_redPosX, 0, GraphicsDevice.Viewport.Width - SpriteDimension);
                _redPosY = MathHelper.Clamp(_redPosY, 0, GraphicsDevice.Viewport.Height- SpriteDimension);
                */
                // clamping

                int screenWidth = GraphicsDevice.Viewport.Width;
                int screenHeight = GraphicsDevice.Viewport.Height;

                // Horizontal wrapping
                if (_redPosX > screenWidth)
                {
                    _redPosX = -SpriteDimension; // Wrap to left side
                }
                else if (_redPosX + SpriteDimension < 0)
                {
                    _redPosX = screenWidth; // Wrap to right side
                }

                // Vertical wrapping
                if (_redPosY > screenHeight)
                {
                    _redPosY = -SpriteDimension; // Wrap to top side
                }
                else if (_redPosY + SpriteDimension < 0)
                {
                    _redPosY = screenHeight; // Wrap to bottom side
                }

                // Horizontal wrapping
                if (_bluePosX > screenWidth)
                {
                    _bluePosX = -SpriteDimension; // Wrap to left side
                }
                else if (_bluePosX + SpriteDimension < 0)
                {
                    _bluePosX = screenWidth; // Wrap to right side
                }

                // Vertical wrapping
                if (_bluePosY > screenHeight)
                {
                    _bluePosY = -SpriteDimension; // Wrap to top side
                }
                else if (_bluePosY + SpriteDimension < 0)
                {
                    _bluePosY = screenHeight; // Wrap to bottom side
                }

                if (!_inExplosion)
                {
                    _spriteBatch.Draw(_blueShip, new Vector2(_bluePosX, _bluePosY), _sourceRectangles[_currentAnimationIndexBlue], Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                }
                else if (_currentExplosionFrameIndex <= 15)
                {
                    _spriteBatch.Draw(_explosion, new Vector2(_bluePosX, _bluePosY), animationFrames[_currentExplosionFrameIndex], Color.White);
                }

                if (!_inExplosionRed)
                {
                    _spriteBatch.Draw(_redShip, new Vector2(_redPosX, _redPosY), _sourceRectangles[_currentAnimationIndex], Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                }
                else if (_currentExplosionFrameIndex <= 15)
                {
                    _spriteBatch.Draw(_explosion, new Vector2(_redPosX, _redPosY), animationFrames[_currentExplosionFrameIndex], Color.White);
                }
                //Console.WriteLine($"Drawing ship at angle {_translations[_currentAnimationIndex]} degrees at position {_redPosX},{_redPosY} with source rectangle {_sourceRectangles[_currentAnimationIndex]}");
                    _spriteBatch.End();

                base.Draw(gameTime);
                break;
            case GameState.Paused:
                // Draw paused screen
                break;
            case GameState.GameOver:
                // Draw game over screen
                break;
        }

        
    }
}
