using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

using System;

namespace Rhythm_game
{  

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static Random Random;

        void SetMusicVolume(float vol)
        {
            MediaPlayer.Volume = vol; 
        }
        List<SoundEffect> soundEffects;


        Random rnd = new Random();

        Texture2D Key1;
        Texture2D Key2;
        Texture2D Key3;
        Texture2D Key4;
        Texture2D StageLeft;
        Texture2D StageRight;
        Texture2D textureScore;
        Texture2D textureSong;
        Texture2D textureMiss;
        Texture2D texture50;
        Texture2D texture100;
        Texture2D texture200;
        Texture2D texture300;
        Texture2D texture300g;
        Texture2D Pause128;
        Texture2D Balck;

        Texture2D testNote; //do testowania
        Texture2D testNote1;
        Texture2D testNote2;

        const int numberOfNotes = 10000;
        struct Note
        {
            public bool canbePressed;
            public bool isAlive;
            public int noteColumn; //column 1-4
            public Vector2 notePosition;
            public Texture2D noteTexture;
        }
        Note[] Notes = new Note[numberOfNotes];


        const int numberOfPieces = 2;
        struct Pieces
        {
            public Texture2D songsTexture;
            public Vector2 songsPosition;
            public float songsBPM;
            public float songsDuration;
            public Song songsPiece;
            public string songsTitle;
        }
        Pieces[] Songs  = new Pieces[numberOfPieces];

        float bpm;
        float tempo;
        bool hasStarted;
        float score;
        float timer;
        float timerShown;
        int currentCombo;
        float volume = 0.1f;
        public int songPlayed;

        public Vector2 note1Position;
        public Vector2 note2Position;
        public Vector2 note3Position;
        public Vector2 note4Position;

        //font
        SpriteFont gameFont;
        SpriteFont gameFont2;

        KeyboardState _currentKeyboardState;
        KeyboardState _previousKeyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            soundEffects = new List<SoundEffect>();
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();



            Songs[0].songsTexture = Content.Load<Texture2D>("BLACKorWHITETexture");
            Songs[0].songsPosition = new Vector2(50, 150);
            Songs[0].songsBPM = 185f;
            Songs[0].songsDuration = 60 * 350;
            Songs[0].songsPiece = Content.Load<Song>("BLACKorWHITE");
            Songs[0].songsTitle = "Black or White?";

            Songs[1].songsTexture = Content.Load<Texture2D>("MittsiesTexture");
            Songs[1].songsPosition = new Vector2(50, 150);
            Songs[1].songsBPM = 150f;
            Songs[1].songsDuration = 60 * 227;
            Songs[1].songsPiece = Content.Load<Song>("MittsiesVitality");
            Songs[1].songsTitle = "Mittsies - Vitality";

            timer = Songs[0].songsDuration;
            songPlayed = 0;
            currentCombo = 0;
            score = 0;
            //Notes position
            note1Position.X = 512; //column1
            note2Position.X = 576; //column2
            note3Position.X = 640; //column3
            note4Position.X = 704; //column4
            
            //Sound effect Voulme
            SoundEffect.MasterVolume = 0.1f;
            MediaPlayer.Volume = 0.1f;

            for (int i = 0; i < numberOfNotes; i++)
            {
                Notes[i].canbePressed = false;
                Notes[i].isAlive = true;
                Notes[i].noteTexture = Content.Load<Texture2D>("mania-note1");
                Notes[i].noteColumn = rnd.Next(1, 5);

                if(Notes[i].noteColumn == 1)
                {
                    Notes[i].notePosition.X = note1Position.X;
                }
                else if (Notes[i].noteColumn == 2)
                {
                    Notes[i].notePosition.X = note2Position.X;
                }
                else if (Notes[i].noteColumn == 3)
                {
                    Notes[i].notePosition.X = note3Position.X;
                }
                else
                {
                    Notes[i].notePosition.X = note4Position.X;
                }

                Notes[i].notePosition.Y = _graphics.PreferredBackBufferHeight - 180*(i)-21-144; //180p difference between notes, i number of note,  - 21 note height texture -144 to align first note with tempo
            }    

            base.Initialize();
        }

        protected override void LoadContent()
        // TODO: use this.Content to load your game content here
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Key1 = Content.Load<Texture2D>("mania-key1");
            Key2 = Content.Load<Texture2D>("mania-key1");
            Key3 = Content.Load<Texture2D>("mania-key1");
            Key4 = Content.Load<Texture2D>("mania-key1");

            StageLeft = Content.Load<Texture2D>("mania-stage-left");
            StageRight = Content.Load<Texture2D>("mania-stage-right");

            textureMiss = Content.Load<Texture2D>("mania-hit0_256");
            texture50 = Content.Load<Texture2D>("mania-hit50_256");
            texture100 = Content.Load<Texture2D>("mania-hit100_256");
            texture200 = Content.Load<Texture2D>("mania-hit200_256");
            texture300 = Content.Load<Texture2D>("mania-hit300_256");
            texture300g = Content.Load<Texture2D>("mania-hit300g_256");
            Pause128 = Content.Load<Texture2D>("pause-128");
            Balck = Content.Load<Texture2D>("Balck");


            soundEffects.Add(Content.Load<SoundEffect>("key-press-1"));


            testNote = Content.Load<Texture2D>("mania-note1");
            testNote1 = Content.Load<Texture2D>("mania-note1");
            testNote2 = Content.Load<Texture2D>("mania-note1");


            gameFont = Content.Load<SpriteFont>("galleryFont");
            gameFont2 = Content.Load<SpriteFont>("galleryFont2");

        }

        protected override void Update(GameTime gameTime)
        {
            //Keyboard state start
            //Escape exists the game
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            _currentKeyboardState = Keyboard.GetState();



            //Changing songs with Arrow left Arrow right
            if ((_currentKeyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left)) && hasStarted == false || (_currentKeyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right)) && hasStarted == false)
            {
                if (songPlayed == 0)
                {
                    songPlayed = 1;
                    timer = Songs[songPlayed].songsDuration;
                    MediaPlayer.Play(Songs[songPlayed].songsPiece);

                }
                else if (songPlayed == 1)
                {
                    songPlayed = 0;
                    timer = Songs[songPlayed].songsDuration;
                    MediaPlayer.Play(Songs[songPlayed].songsPiece);
                    
                }
            }
            //Makes sure music plays
            if (MediaPlayer.State == MediaState.Stopped) MediaPlayer.Play(Songs[songPlayed].songsPiece);
            else if (MediaPlayer.State == MediaState.Paused) MediaPlayer.Resume();


            //Restart
            if (_currentKeyboardState.IsKeyDown(Keys.R) && _previousKeyboardState.IsKeyUp(Keys.R))
            {
                hasStarted = false;
                score = 0;
                currentCombo = 0;
                timer = Songs[songPlayed].songsDuration;

                for (int i = 0; i < numberOfNotes; i++)
                {
                    Notes[i].canbePressed = false;
                    Notes[i].isAlive = true;
                    Notes[i].noteTexture = Content.Load<Texture2D>("mania-note1");
                    Notes[i].noteColumn = rnd.Next(1, 5);

                    if (Notes[i].noteColumn == 1)
                    {
                        Notes[i].notePosition.X = note1Position.X;
                    }
                    else if (Notes[i].noteColumn == 2)
                    {
                        Notes[i].notePosition.X = note2Position.X;
                    }
                    else if (Notes[i].noteColumn == 3)
                    {
                        Notes[i].notePosition.X = note3Position.X;
                    }
                    else
                    {
                        Notes[i].notePosition.X = note4Position.X;
                    }

                    Notes[i].notePosition.Y = _graphics.PreferredBackBufferHeight - 180 * (i) - 21 - 144; //180p difference between notes, i number of note,  - 21 note height texture -144 to align first note with tempo
                }

            }



            //Adjusting bpm depending on the song
            if (songPlayed == 0)
            {
                bpm = Songs[0].songsBPM;
                textureSong = Songs[0].songsTexture;
            }
            else if (songPlayed == 1)
            {
                bpm = Songs[1].songsBPM;
                textureSong = Songs[1].songsTexture;
            }


            tempo = bpm / 60;


            //Volume Arrow up Arrow down

            if(_currentKeyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up))
            {
                volume = volume + 0.05f;
                SetMusicVolume(volume);
            }

            if (_currentKeyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down))
            {
                volume = volume - 0.05f;
                SetMusicVolume(volume);
            }

            // Button Pressed texture change
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Key1 = Content.Load<Texture2D>("mania-Key1D");
            }

            else
            {
                Key1 = Content.Load<Texture2D>("mania-Key1");
            }


            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                Key2 = Content.Load<Texture2D>("mania-Key1D");
            }

            else
            {
                Key2 = Content.Load<Texture2D>("mania-Key1");
            }


            if (Keyboard.GetState().IsKeyDown(Keys.J))
            {
                Key3 = Content.Load<Texture2D>("mania-Key1D");
            }

            else
            {
                Key3 = Content.Load<Texture2D>("mania-Key1");
            }


            if (Keyboard.GetState().IsKeyDown(Keys.K))
            {
                Key4 = Content.Load<Texture2D>("mania-Key1D");
            }

            else
            {
                Key4 = Content.Load<Texture2D>("mania-Key1");
            }

            //Playing key sound effect on press
            if (_currentKeyboardState.IsKeyDown(Keys.D) && _previousKeyboardState.IsKeyUp(Keys.D))
            {
                soundEffects[0].CreateInstance().Play();
            }

            if (_currentKeyboardState.IsKeyDown(Keys.F) && _previousKeyboardState.IsKeyUp(Keys.F))
            {
                soundEffects[0].CreateInstance().Play();
            }

            if (_currentKeyboardState.IsKeyDown(Keys.J) && _previousKeyboardState.IsKeyUp(Keys.J))
            {
                soundEffects[0].CreateInstance().Play();
            }

            if (_currentKeyboardState.IsKeyDown(Keys.K) && _previousKeyboardState.IsKeyUp(Keys.K))
            {
                soundEffects[0].CreateInstance().Play();
            }
            //Unpausing the game by pressing space, speed of notes scrolling dependant on tempo
            if (!hasStarted)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space) && Notes[0].isAlive == false)
                {
                    hasStarted = true;
                    
                    MediaPlayer.Play(Songs[songPlayed].songsPiece);
                }
            }
            else
            {
                for (int i = 0; i < numberOfNotes; i++)
                {
                    Notes[i].notePosition.Y += 180/60*(bpm/60); //180piksels per frame / 60 frames * (beats per second)
                }

            }

            //can you click on notes
            for (int i = 0; i < numberOfNotes; i++)
            {
                if(Notes[i].notePosition.Y >=465  && Notes[i].notePosition.Y <= 645 && Notes[i].isAlive==true)
                {
                    Notes[i].canbePressed = true;
                }
                else Notes[i].canbePressed = false;
            }

            //buttons interracting with notes
            for (int i = 0; i < numberOfNotes; i++)
            {
                if(Notes[i].canbePressed == true && Notes[i].noteColumn == 1 && _currentKeyboardState.IsKeyDown(Keys.D) && _previousKeyboardState.IsKeyUp(Keys.D))
                {
                    Notes[i].isAlive = false;
                }
            }
            for (int i = 0; i < numberOfNotes; i++)
            {
                if (Notes[i].canbePressed == true && Notes[i].noteColumn == 2 && _currentKeyboardState.IsKeyDown(Keys.F) && _previousKeyboardState.IsKeyUp(Keys.F))
                {
                    Notes[i].isAlive = false;
                }
            }
            for (int i = 0; i < numberOfNotes; i++)
            {
                if (Notes[i].canbePressed == true && Notes[i].noteColumn == 3 && _currentKeyboardState.IsKeyDown(Keys.J) && _previousKeyboardState.IsKeyUp(Keys.J))
                {
                    Notes[i].isAlive = false;
                }
            }
            for (int i = 0; i < numberOfNotes; i++)
            {
                if (Notes[i].canbePressed == true && Notes[i].noteColumn == 4 && _currentKeyboardState.IsKeyDown(Keys.K) && _previousKeyboardState.IsKeyUp(Keys.K))
                {
                    Notes[i].isAlive = false;
                }
            }
            //Adding to score and showing hit texture

            for (int i = 0; i < numberOfNotes; i++)
            {
                if (Notes[i].canbePressed == true && Notes[i].isAlive == false)
                {
                    if (Notes[i].notePosition.Y < 555 + 10 && Notes[i].notePosition.Y > 555 - 10)
                    {
                        score = score + 320 + (currentCombo + 1);
                        currentCombo = currentCombo + 1;
                        textureScore = Content.Load<Texture2D>("mania-hit300g_256");
                    }
                    else if ((Notes[i].notePosition.Y < 555 + 30 && Notes[i].notePosition.Y >= 555 + 10) || (Notes[i].notePosition.Y > 555 - 30 && Notes[i].notePosition.Y <= 555 - 10))
                    {
                        score = score + 300 + (currentCombo + 1);
                        currentCombo = currentCombo + 1;
                        textureScore = Content.Load<Texture2D>("mania-hit300_256");
                    }
                    else if ((Notes[i].notePosition.Y < 555 + 50 && Notes[i].notePosition.Y >= 555 + 30) || (Notes[i].notePosition.Y > 555 - 50 && Notes[i].notePosition.Y <= 555 - 30))
                    {
                        score = score + 200 + (currentCombo + 1);
                        currentCombo = currentCombo + 1;
                        textureScore = Content.Load<Texture2D>("mania-hit200_256");
                    }
                    else if ((Notes[i].notePosition.Y < 555 + 70 && Notes[i].notePosition.Y >= 555 + 50) || (Notes[i].notePosition.Y > 555 - 70 && Notes[i].notePosition.Y <= 555 - 50))
                    {
                        score = score + 100 + (currentCombo + 1);
                        currentCombo = currentCombo + 1;
                        textureScore = Content.Load<Texture2D>("mania-hit100_256");
                    }
                    else if ((Notes[i].notePosition.Y < 555 + 80 && Notes[i].notePosition.Y >= 555 + 70) || (Notes[i].notePosition.Y > 555 - 80 && Notes[i].notePosition.Y <= 555 - 70))
                    {
                        score = score + 50 + (currentCombo + 1);
                        currentCombo = currentCombo + 1;
                        textureScore = Content.Load<Texture2D>("mania-hit50_256");
                    }
                    else 
                    {
                        score = score + 1;
                        currentCombo = 0;
                        textureScore = Content.Load<Texture2D>("mania-hit0_256");
                    }
                }
            }

            //perfect hit when note.Position.Y = 555, notes can be pressed between 445 and 645

            //killing notes that are not hit
            for (int i = 0; i < numberOfNotes; i++)
            {
                if (Notes[i].canbePressed == false && Notes[i].isAlive == true && Notes[i].notePosition.Y >= 555 + 80)
                {
                    currentCombo = 0;
                    Notes[i].isAlive = false;
                    textureScore = Content.Load<Texture2D>("mania-hit0_256");
                }
            }
            //timer

            timerShown = timer / 60f;
            if (hasStarted)
            {
                timer = timer - 1;
            }

            if (timer == 0)
            {
                hasStarted = false;
                
            }
           
                //Set the previous keyboard state to the current one
                _previousKeyboardState = _currentKeyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        // TODO: Add your drawing code here
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            //Background
            _spriteBatch.Draw(Songs[songPlayed].songsTexture, new Vector2(0, 0), Color.White);
            _spriteBatch.Draw(Balck, new Vector2(512, 0), Color.White);

            //_spriteBatch.Draw(testNote, new Vector2(512, 555), Color.White); //will start dying at Y=624 (half of the button texture)
            //_spriteBatch.Draw(testNote1, new Vector2(512, 555+90), Color.White); //will start dying at Y=624 (half of the button texture)
            //_spriteBatch.Draw(testNote2, new Vector2(512, 555-90), Color.White); //will start dying at Y=624 (half of the button texture)

            //when note dies it becomes invisible
            for (int i = 0; i < numberOfNotes; i++)
            {   
                if(Notes[i].isAlive==false)
                {
                    _spriteBatch.Draw(Notes[i].noteTexture, Notes[i].notePosition, Color.Black);
                }
                else
                {
                    _spriteBatch.Draw(Notes[i].noteTexture, Notes[i].notePosition, Color.White);
                }
                
            }
            if(songPlayed == 1)
            {
                _spriteBatch.DrawString(gameFont, "Score: " + string.Format("{0:N0}", score), new Vector2(1000, 10), Color.Silver);
                _spriteBatch.DrawString(gameFont, "BPM: " + string.Format("{0:N1}", bpm), new Vector2(800, 10), Color.Silver);
            }
            else if (songPlayed == 0)
            {
                _spriteBatch.DrawString(gameFont, "Score: " + string.Format("{0:N0}", score), new Vector2(1000, 10), Color.Black);
                _spriteBatch.DrawString(gameFont, "BPM: " + string.Format("{0:N1}", bpm), new Vector2(800, 10), Color.Black);
            }


            //combo
            if (currentCombo < 10)
            {
                _spriteBatch.DrawString(gameFont, string.Format("{0:N0}", currentCombo), new Vector2(630, 181), Color.Silver);
            }
            else if (currentCombo < 100)
            {
                _spriteBatch.DrawString(gameFont, string.Format("{0:N0}", currentCombo), new Vector2(620, 181), Color.Silver);
            }
            else if (currentCombo < 1000)
            {
                _spriteBatch.DrawString(gameFont, string.Format("{0:N0}", currentCombo), new Vector2(610, 181), Color.Silver);
            }
            else
            {
                _spriteBatch.DrawString(gameFont, string.Format("{0:N0}", currentCombo), new Vector2(600, 181), Color.Silver);
            }


            //once song ends you can press space to continue for infinite mode
            if(songPlayed == 1)
            {
                if (timer >= 0)
                {
                    _spriteBatch.DrawString(gameFont, "Time: " + string.Format("{0:N0}", timerShown) + " / " + string.Format("{0:N0}", Songs[songPlayed].songsDuration / 60f), new Vector2(800, 50), Color.Silver);
                }
                else
                {
                    _spriteBatch.DrawString(gameFont, "Infinite mode", new Vector2(800, 60), Color.Silver);
                }
            }

            else if (songPlayed == 0)
            {
                if (timer >= 0)
                {
                    _spriteBatch.DrawString(gameFont, "Time: " + string.Format("{0:N0}", timerShown) + " / " + string.Format("{0:N0}", Songs[songPlayed].songsDuration / 60f), new Vector2(800, 50), Color.Black);
                }
                else
                {
                    _spriteBatch.DrawString(gameFont, "Infinite mode", new Vector2(800, 60), Color.Black);
                }
            }


            _spriteBatch.Draw(Key1, new Vector2(512, 528), Color.White);
            _spriteBatch.Draw(Key2, new Vector2(576, 528), Color.White);
            _spriteBatch.Draw(Key3, new Vector2(640, 528), Color.White);
            _spriteBatch.Draw(Key4, new Vector2(704, 528), Color.White);
            _spriteBatch.Draw(StageLeft, new Vector2(464, -5), Color.White);
            _spriteBatch.Draw(StageRight, new Vector2(768, -5), Color.White);



            if(Notes[0].isAlive == false)
            {
                _spriteBatch.Draw(textureScore, new Vector2(512, 100), Color.White);
            }



            //tutorial
            if (!hasStarted)
            {
                _spriteBatch.Draw(Pause128, new Vector2(576, 220), Color.Silver);


                _spriteBatch.DrawString(gameFont2, "             Hit first note\n                and press\n            SPACE to start.\n          Press R to Reset.", new Vector2(535, 350), Color.Silver);
                if (songPlayed == 1)
                {
                    _spriteBatch.DrawString(gameFont2, "Adjust volume by pressing\nArrow UP and Arrow Down.", new Vector2(820, 620), Color.Silver);
                    _spriteBatch.DrawString(gameFont2, "Volume: " + string.Format("{0:N2}", volume), new Vector2(820, 680), Color.Silver);
                    _spriteBatch.DrawString(gameFont2, "Change song by pressing\nArrow Left or Arrow Right.", new Vector2(820, 530), Color.Silver);
                    _spriteBatch.DrawString(gameFont2, "Song: " + Songs[songPlayed].songsTitle, new Vector2(820, 590), Color.Silver);
                }
                else if(songPlayed == 0)
                {
                    _spriteBatch.DrawString(gameFont2, "Adjust volume by pressing\nArrow UP and Arrow Down.", new Vector2(820, 620), Color.Black);
                    _spriteBatch.DrawString(gameFont2, "Volume: " + string.Format("{0:N2}", volume), new Vector2(820, 680), Color.Black);
                    _spriteBatch.DrawString(gameFont2, "Change song by pressing\nArrow Left or Arrow Right.", new Vector2(820, 530), Color.Black);
                    _spriteBatch.DrawString(gameFont2, "Song: " + Songs[songPlayed].songsTitle, new Vector2(820, 590), Color.Black);
                }

                _spriteBatch.DrawString(gameFont, "D", new Vector2(528, 605), Color.Silver);
                _spriteBatch.DrawString(gameFont, "F", new Vector2(596, 605), Color.Silver);
                _spriteBatch.DrawString(gameFont, "J", new Vector2(668, 605), Color.Silver);
                _spriteBatch.DrawString(gameFont, "K", new Vector2(724, 605), Color.Silver);

            }


            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
