using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using System.Collections.Generic;
using TGC.Core.Collision;
using System.Reflection;
using System;
using TGC.Core.Sound;
using TGC.Group.Model.Escenarios;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
       
        
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

     //   private TgcScene scene;

        public override void Init(){
            var menu = new Menu();
            AdministradorDeEscenarios.getSingleton().setMediaDir(MediaDir);
            AdministradorDeEscenarios.getSingleton().setShaderDir(ShadersDir);
            AdministradorDeEscenarios.getSingleton().agregarEscenario(menu,Camara);
        }


        public override void Update()
        {
            PreUpdate();

            if (ElapsedTime < 0.02)
            {
                AdministradorDeEscenarios.getSingleton().update(ElapsedTime, Input, Camara);
                if (AdministradorDeEscenarios.getSingleton().GetCamaraEscenario() != null)
                    Camara = AdministradorDeEscenarios.getSingleton().GetCamaraEscenario();
            }

            PostUpdate();
        }

        public override void Render(){

            PreRender();

            AdministradorDeEscenarios.getSingleton().render(ElapsedTime);

            PostRender();

        }

        public override void Dispose()
        {
            AdministradorDeEscenarios.getSingleton().dispose();
        }

    }
}