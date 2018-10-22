using TGC.Group.Model.Interfaz;
using TGC.Core.Textures;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Camara;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using System.Drawing;
using Microsoft.DirectX;
using TGC.Core.Sound;

namespace TGC.Group.Model.Escenarios
{
    class GameOver : Escenario
    {
        private Boton volver;

        private TgcTexture gameOver;
        private TgcMp3Player reproductorMp3 = new TgcMp3Player();
        private Sprite sprite;
        private Viewport vp = D3DDevice.Instance.Device.Viewport;
        private string pathDeLaCancion;

        public void init(string mediaDir, string shaderDir, TgcCamera camara)
        {
            reproductorMp3.closeFile();
            pathDeLaCancion = mediaDir + "Musica\\gameover.mp3";
            reproductorMp3.FileName = pathDeLaCancion;
            volver = new Boton("Volver al Menú principal", 0f, 0.8f, () => { reproductorMp3.closeFile(); AdministradorDeEscenarios.getSingleton().agregarEscenario(new Menu(), camara); });
            sprite = new Sprite(D3DDevice.Instance.Device);
            gameOver = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + "gameover.jpg");
            reproductorMp3.play(true);
        }

        public void update(float deltaTime, TgcD3dInput input, TgcCamera camara)
        {
            volver.Update(deltaTime, input);
            
        }


        public void render(float deltaTime)
        {

            sprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);

            var scaling = new TGCVector3((float)vp.Width / gameOver.Width, (float)vp.Height / gameOver.Height, 0);

            sprite.Transform = TGCMatrix.Scaling(scaling);
            sprite.Draw(gameOver.D3dTexture, Rectangle.Empty, Vector3.Empty, Vector3.Empty, Color.White);

            sprite.End();

            volver.Render();
        }

        public void dispose()
        {
            volver.Dispose();
            reproductorMp3.closeFile();
        }


    }
}
