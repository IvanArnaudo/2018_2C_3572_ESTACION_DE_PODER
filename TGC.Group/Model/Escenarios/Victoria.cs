using TGC.Group.Model.Interfaz;
using TGC.Core.Textures;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Camara;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using System.Drawing;
using Microsoft.DirectX;

namespace TGC.Group.Model.Escenarios
{
    class Victoria : Escenario{
        private Boton volver;
        private Boton texto;
        private TgcTexture victory;
        private Sprite sprite;
        private Viewport vp = D3DDevice.Instance.Device.Viewport;

        public void init(string mediaDir, string shaderDir, TgcCamera camara){
            volver = new Boton("Volver al Menú principal", 0f, 0.8f, () => AdministradorDeEscenarios.getSingleton().agregarEscenario(new Menu(), camara));
            texto = new Boton("Felicidades, aprobaste 2 materias ¿Hasta dónde llegarás?", 0f, 0.7f, null);
            sprite = new Sprite(D3DDevice.Instance.Device);
            victory = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + "victory.jpg");
        }

        public void update(float deltaTime, TgcD3dInput input, TgcCamera camara){
            volver.Update(deltaTime, input);
        }


        public void render(float deltaTime){
            sprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);

            var scaling = new TGCVector3((float)vp.Width / victory.Width, (float)vp.Height / victory.Height, 0);

            sprite.Transform = TGCMatrix.Scaling(scaling);
            sprite.Draw(victory.D3dTexture, Rectangle.Empty, Vector3.Empty, Vector3.Empty, Color.White);

            sprite.End();

            volver.Render();
            texto.Render();
        }

        public void dispose(){
            volver.Dispose();
            texto.Dispose();
        }


    }
}
