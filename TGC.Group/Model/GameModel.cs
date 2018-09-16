using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Examples.Camara;

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
        private const float MOVEMENT_SPEED = 200;
        private TgcMesh robot;
        private TgcThirdPersonCamera camara_interna;
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

       // private TgcMesh mesh;
        private TgcScene scene;

        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Acá empieza mi intento de insertar una escena
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "ParadigmasEscena\\nivelParadigmas13-TgcScene.xml");
            robot = scene.Meshes[65];
            robot.AutoTransform = true;
            camara_interna = new TgcThirdPersonCamera(robot.BoundingBox.calculateBoxCenter(),  300, 300);
            Camara = camara_interna;
            camara_interna.rotateY(Geometry.DegreeToRadian(90));
        }
        public override void Update()
        {
            PreUpdate();
            var input = Input;
            var movement = TGCVector3.Empty;
            var originalPos = robot.Position;

            movement.X = movimiento_izquierda(input) + movimiento_derecha(input);
            movement.Z = movimiento_arriba(input) + movimiento_abajo(input);
            movement *=  ElapsedTime;
            robot.Move(movement);

            camara_interna.Target = robot.Position;

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();
            scene.RenderAll();
            PostRender();
        }
        public override void Dispose()
        {
            scene.DisposeAll(); //Dispose de la escena.
        }

        private float movimiento_izquierda(TgcD3dInput input)
        {
            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
                return MOVEMENT_SPEED;
            return 0;
        }
        private float movimiento_derecha(TgcD3dInput input)
        {
            if (input.keyDown(Key.Right) || input.keyDown(Key.D))
                return -MOVEMENT_SPEED;
            return 0;
        }
        private float movimiento_abajo(TgcD3dInput input)
        {
            if (input.keyDown(Key.Down) || input.keyDown(Key.S))
                return MOVEMENT_SPEED;
            return 0;
        }
        private float movimiento_arriba(TgcD3dInput input)
        {
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
                return -MOVEMENT_SPEED;
            return 0;
        }
    }
}