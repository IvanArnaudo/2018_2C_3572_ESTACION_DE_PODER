/*using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Input;
using TGC.Core.Text;
using System.Drawing;


namespace TGC.Group.Model.Interfaz
{

    public abstract class Menu : IRenderObject
    {
        //Protected para que puedan ser utilizados por las clases que heredan de Menu
        protected TgcText2D texto;
        protected Font fuente = new Font("Fixedsys", 30, FontStyle.Regular, GraphicsUnit.Pixel); //"Lucida Console" "Impact" "Fixedsys" "Arial"
        protected Size tamanio;

        //esto es para poder heredar de IRenderObject
        public bool AlphaBlendEnable { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public Menu(string content, float posX, float posY){

            texto = new TgcText2D{
                Text = content,
                Color = Color.White,
            };

            texto.changeFont(fuente);

            tamanio = TextRenderer.MeasureText(texto.Text, fuente);
            tamanio.Width = (int)(tamanio.Width/* * 1.5*///); MIRA QUE ACA LE AGREGUE DOS BARRAS AL FINAL PARA COMENTAR TODO.

            /*texto.Size = tamanio;

            var vp = D3DDevice.Instance.Device.Viewport;
            texto.Position = new Point(
                (int)((posX * vp.Width)),
                (int)((posY * vp.Height))
            );
        }

        public Rectangle getBounds(){
            return new Rectangle{
                X = texto.Position.X,
                Y = texto.Position.Y,
                Width = tamanio.Width,
                Height = tamanio.Height
            };
        }

        public abstract void Update(float deltaTime, TgcD3dInput input);

        public void Render(){
            texto.render();
        }

        public void Dispose() {
            texto.Dispose();
        }


    }
}

*/