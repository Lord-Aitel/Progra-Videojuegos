using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2DEngine
{
    public class P2DGame
    {
        // Ventana que se va a mostrar.
        P2DWindow mainWindow;

        // Posicion del rectángulo en la pantalla.
        private int rectangleX { get; set; }
        private int rectangleY { get; set; }

        // Posicion de la IA en la pantalla
        private int rectangleAIX { get; set; }
        private int rectangleAIY { get; set; }

        // Posición del círculo en la pantalla.
        public int ball_x { get; set; }
        public int ball_y { get; set; }

        // Velocidad del círculo en el eje x e y.
        public int ball_dx { get; set; }
        public int ball_dy { get; set; }

        // Velocidad de la IA.
        public int aiSpeed { get; set; }

        // Puntuación de los jugadores.
        public int playerScore { get; set; }
        public int aiScore { get; set; }

        // Es recomentable utilizar el constructor para inicializar las variables.
        public P2DGame(int width, int height)
        {
            mainWindow = new P2DWindow(width, height);

            // Centramos la pelota.
            ball_x = width / 2;
            ball_y = height / 2;

            // La velocidad inicial.
            ball_dx = 1;
            ball_dy = 1;

            // Inicializar posición y velocidad de la IA.
            rectangleAIX = width - 20;
            rectangleAIY = height / 2 - 50;
            aiSpeed = 6;

            // Inicializar puntuaciones.
            playerScore = 10;
            aiScore = 0;
        }

        public void Start()
        {
            mainWindow.Show(); // Mostramos nuestra ventana.

            Thread t = new Thread(GameLoop); // Generamos un hilo con el gameloop en otro hilo.
            t.Start(); // Iniciamos el gameloop.
        }

        public void GameLoop()
        {
            bool loop = true;

            while (loop) // Mientras siga corriendo el programa.
            {
                Stopwatch sw = new Stopwatch(); // Utilizado para medir el tiempo.

                sw.Start();
                // Estos son los elementos principales del game loop.
                ProcessInput(); // Procesamos los inputs
                UpdateGame(); // Actualizamos el estado del juego.
                RenderGame(); // Mostramos en pantalla.
                sw.Stop();

                int deltaTime = (int)sw.ElapsedMilliseconds; // Tiempo que demora en una iteración del loop.

                int sleepTime = (1000 / 60) - deltaTime; // Cuanto debe "dormir" 

                if (sleepTime < 0) // Dejaremos que el loop duerma mínimo un milisegundo.
                {
                    sleepTime = 1;
                }
                Thread.Sleep(sleepTime);

                if (mainWindow.IsDisposed) // Si cerramos la ventana, se cierra el juego.
                {
                    loop = false;
                }
            }

            Environment.Exit(0); // Propio de Forms.
        }

        private void RenderGame() // Lógica de dibujado.
        {
            Graphics g = mainWindow.CreateGraphics();

            // Pintamos el fondo.
            g.FillRectangle(new SolidBrush(Color.GreenYellow), 0, 0, mainWindow.ClientSize.Width, mainWindow.ClientSize.Height);

            g.FillRectangle(new SolidBrush(Color.Black), 400, 0, 15, 1000);

            // Rectángulo jugador.
            g.FillRectangle(new SolidBrush(Color.Black), rectangleX, rectangleY, 20, 100);

            // Rectángulo IA.
            g.FillRectangle(new SolidBrush(Color.Black), rectangleAIX, rectangleAIY, 20, 100);

            // Círculo.
            g.FillEllipse(new SolidBrush(Color.Black), ball_x, ball_y, 20, 20);

            //Puntaje
            Font font = new Font("Arial", 16);
            g.DrawString("Player: " + playerScore, font, Brushes.Black, new Point(300, 10));
            g.DrawString("AI: " + aiScore, font, Brushes.Black, new Point(mainWindow.ClientSize.Width - 380, 10));
        }

        private void UpdateGame() // Lógica de juego.
        {
            // Cuantas unidades queremos que se mueva.
            int step = 10;
            // Guardar la posición anterior de la pelota
            int previousBallY = ball_y;

            // Actualizar la posición de la pelota
            ball_x += ball_dx;
            ball_y += ball_dy;

            // Movimiento de la IA
            if (ball_y > rectangleAIY + 50)
            {
                rectangleAIY += aiSpeed; // Mueve hacia abajo
            }
            else if (ball_y < rectangleAIY + 50)
            {
                rectangleAIY -= aiSpeed; // Mueve hacia arriba
            }

            // Restaurar la posición de la pelota a la posición anterior
            ball_y = previousBallY;

            // Detectar colisión con los rectángulos.
            if (ball_x + 20 >= rectangleX && ball_x <= rectangleX + 20 &&
                ball_y + 20 >= rectangleY && ball_y <= rectangleY + 100)
            {
                // La pelota está colisionando con el rectángulo jugador, así que cambiamos la dirección en X.
                ball_dx *= -1;
            }

            if (ball_x + 20 >= rectangleAIX && ball_x <= rectangleAIX + 20 &&
                ball_y + 20 >= rectangleAIY && ball_y <= rectangleAIY + 100)
            {
                // La pelota está colisionando con el rectángulo IA, así que cambiamos la dirección en X.
                ball_dx *= -1;
            }

            // Movemos el balón.
            ball_x += ball_dx * step;
            ball_y += ball_dy * step;

            // Queremos que rebote en los bordes.
            if (ball_x < 0)
            {
                aiScore++;
                ball_dx = 1;
                CheckGameEnd();
            }
            else if (ball_x > mainWindow.ClientSize.Width)
            {
                playerScore++;
                ball_dx = -1;
                CheckGameEnd();
            }

            if (ball_y < 0)
            {
                ball_dy = 1;
            }
            else if (ball_y > mainWindow.ClientSize.Height)
            {
                ball_dy = -1;
            }

        }

private void CheckGameEnd()
{
    if (playerScore >= 11 || aiScore >= 11)
    {
        EndGame();
    }
}

private void EndGame()
{
    // Utiliza Invoke para asegurar el acceso a los controles de Windows Forms desde el hilo principal.
    mainWindow.Invoke((MethodInvoker)delegate
    {
        // Verifica si la ventana aún no ha sido eliminada.
        if (!mainWindow.IsDisposed)
        {
            // Aquí puedes agregar lógica adicional para manejar el final del juego, como mostrar un mensaje de ganador, reiniciar el juego, etc.
            MessageBox.Show((playerScore >= 11) ? "Ganaste" : "Perdiste", "Fin del juego");

            // Cierra la ventana.
            mainWindow.Close();

            // Sale de la aplicación.
            Application.Exit();
        }
    });
}

        private void ProcessInput() // Procesamos la entrada que realiza el usuario.
        {
            int step = 10;

            if (mainWindow.IsKeyPressed(Keys.Up))
            {
                if (rectangleY - step >= 0)
                {
                    rectangleY -= step; // Movemos hacia arriba.
                }
            }
            if (mainWindow.IsKeyPressed(Keys.Down))
            {
                if (rectangleY + 100 + step <= mainWindow.ClientSize.Height)
                {
                    rectangleY += step; // Movemos hacia abajo.
                }
            }
        }
    }
}
