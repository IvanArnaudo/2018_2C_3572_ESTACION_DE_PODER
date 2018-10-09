using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Camara;
using TGC.Core.Input;

namespace TGC.Group.Model.Escenarios
{
    class AdministradorDeEscenarios
    {

        private List<Escenario> escenarios;
        private static AdministradorDeEscenarios singleton;
        private string mediaDir;
        private string shaderDir;

        private int escenarioActual;
        private int escenarioSiguiente;
       
        private AdministradorDeEscenarios(){
            escenarios = new List<Escenario>();
        }

        public static AdministradorDeEscenarios getSingleton(){
            if (singleton == null){
                singleton = new AdministradorDeEscenarios();
            }

            return singleton;
        }


        public void agregarEscenario(Escenario escenario){
            // escena.init(mediaDir, shaderDir); HAY QUE PONER EL MEDIADIR Y EL SHADERDIR COMO VARIABLES GLOBALES
            escenarios.Add(escenario);
            escenarioSiguiente = escenarios.IndexOf(escenario);
            //proxima = scene;
        }


        public void update(float deltaTime, TgcD3dInput input, TgcCamera camara){
            if (escenarios[escenarioSiguiente] != null){
                escenarioActual = escenarioSiguiente;
                escenarioSiguiente = -1;
            }

            if (escenarioActual == -1) return;

            escenarios[escenarioActual].update(deltaTime, input, camara);
        }

        public void render(float deltaTime){
            if (escenarioActual == -1) return;

            escenarios[escenarioActual].render(deltaTime);
        }

        public void dispose(){

            foreach(Escenario esc in escenarios){
                esc.dispose();
            }
            //while (scenes.Count > 0)
            //{
            //    scenes.Pop().dispose();
            //}
        }

        public void escenarioAnterior(){
            escenarios.Remove(escenarios[escenarioActual]);
            escenarioSiguiente = escenarioActual - 1;
        }

        public void setMediaDir(string mediaDir)
        {
            this.mediaDir = mediaDir;
        }

        public void setShaderDir(string shaderDir)
        {
            this.shaderDir = shaderDir;
        }



    }
}
