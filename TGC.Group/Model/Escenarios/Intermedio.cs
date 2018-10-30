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
using TGC.Core.BoundingVolumes;

namespace TGC.Group.Model.Escenarios
{
    class Intermedio : Escenario
    {
        private Boton volver;
        private Boton texto;
        private TgcTexture victory;
        private Sprite sprite;
        private Viewport vp = D3DDevice.Instance.Device.Viewport;
        private TgcMp3Player reproductorMp3 = new TgcMp3Player();
        private string pathDeLaCancion;


        public void init(string mediaDir, string shaderDir, TgcCamera camara)
        {
            pathDeLaCancion = mediaDir + "Musica\\MissionComplete.mp3";
            reproductorMp3.FileName = pathDeLaCancion;
            reproductorMp3.play(true);

            volver = new Boton("Cursar PdeP", 0f, 0.8f, () => { reproductorMp3.closeFile(); AdministradorDeEscenarios.getSingleton().agregarEscenario(new nivelPDP(), camara); });
            texto = new Boton("Bien, te hiciste todos los ejercicios habidos y por haber ¿Seguimos?", 0f, 0.7f, null);
            sprite = new Sprite(D3DDevice.Instance.Device);
            victory = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + "missionPassed.png");

        }

        public void update(float deltaTime, TgcD3dInput input, TgcCamera camara)
        {
            volver.Update(deltaTime, input);
        }


        public void render(float deltaTime, TgcFrustum frustum)
        {
            sprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);

            var scaling = new TGCVector3((float)vp.Width / victory.Width, (float)vp.Height / victory.Height, 0);

            sprite.Transform = TGCMatrix.Scaling(scaling);
            sprite.Draw(victory.D3dTexture, Rectangle.Empty, Vector3.Empty, Vector3.Empty, Color.White);

            sprite.End();

            volver.Render();
            texto.Render();
        }

        public void dispose()
        {
            volver.Dispose();
            texto.Dispose();
            reproductorMp3.closeFile();

        }

    }
}