using System;
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
    class Menu : Escenario
    {
        //private Action cargarNivel1 = () => AdministradorDeEscenarios.getSingleton().agregarEscenario(new nivelF1(),camara);
        private Boton inicio;
        private Sprite sprite;
        private TgcTexture trustMe;
        private Viewport vp = D3DDevice.Instance.Device.Viewport;

        

        public void init(string mediaDir, string shaderDir, TgcCamera camara)
        {
            inicio = new Boton("Inicio", 0f, 0.8f, () => AdministradorDeEscenarios.getSingleton().agregarEscenario(new nivelPDP(), camara));
            sprite = new Sprite(D3DDevice.Instance.Device);
            trustMe = TgcTexture.createTexture(D3DDevice.Instance.Device, mediaDir + "imgMenu.png");
        }

        public void update(float deltaTime, TgcD3dInput input, TgcCamera camara)
        {
            inicio.Update(deltaTime, input);
        }


        public void render(float deltaTime){
            sprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);

            var scaling = new TGCVector3((float)vp.Width / trustMe.Width, (float)vp.Height / trustMe.Height, 0);

            sprite.Transform = TGCMatrix.Scaling(scaling);
            sprite.Draw(trustMe.D3dTexture, Rectangle.Empty, Vector3.Empty, Vector3.Empty, Color.White);

            //sprite.Transform = TGCMatrix.Translation(new TGCVector3(vp.Width / 2 - logo.Width / 2, 0, 0));
            //sprite.Draw(logo.D3dTexture, Rectangle.Empty, Vector3.Empty, Vector3.Empty, Color.White);

            sprite.End();

            inicio.Render();
        }



        public void dispose()
        {
            inicio.Dispose();
        }


        }


}
