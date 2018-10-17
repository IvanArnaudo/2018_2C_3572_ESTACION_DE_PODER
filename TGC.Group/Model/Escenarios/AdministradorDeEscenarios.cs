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
        private TgcCamera camaraEsc;

        private int escenarioActual=0;
        private int escenarioSiguiente=0;
       
        private AdministradorDeEscenarios(){
            escenarios = new List<Escenario>();
        }

        public static AdministradorDeEscenarios getSingleton(){
            if (singleton == null){
                singleton = new AdministradorDeEscenarios();
            }

            return singleton;
        }


        public void agregarEscenario(Escenario escenario, TgcCamera camara)
        {
            escenario.init(mediaDir, shaderDir, camara);
            escenarios.Add(escenario);
            escenarioSiguiente = escenarios.IndexOf(escenario);
            //proxima = scene;
        }

        public TgcCamera GetCamaraEscenario(){
            return camaraEsc;
        }
        public void SetCamara(TgcCamera camara){
            camaraEsc = camara;
        }

        public void update(float deltaTime, TgcD3dInput input, TgcCamera camara){
            if (escenarios[escenarioSiguiente] != null){
                escenarioActual = escenarioSiguiente;
                escenarioSiguiente = escenarios.Count()-1;
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
