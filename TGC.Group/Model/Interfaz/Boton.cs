using TGC.Core.Input;
using System.Drawing;
using System;

namespace TGC.Group.Model.Interfaz
{
    class Boton : Menu
    {

        Action accion;

        public Boton(string content, float posX, float posY, Action action) : base(content, posX, posY)
        {
            this.accion = action;
        }

        public void cambiarTexto(string text)
        {
            texto.Text = text;
        }

        public override void Update(float deltaTime, TgcD3dInput input)
        {
            //si el mouse pasa por encima del boton:
            if (input.Xpos >= getBounds().X && 
                input.Xpos <= getBounds().X + getBounds().Width &&
                 
                input.Ypos >= getBounds().Y &&
                input.Ypos <= getBounds().Y + getBounds().Height)
            {
                texto.Color = Color.Red;
                if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    accion();
                }
            }
            else
            {
                texto.Color = Color.White;
            }
        }



    }
}
