using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Drawing;
using TGC.Core.Collision;
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
        TGCVector3 vectorCamara = new TGCVector3();
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
            scene = loader.loadSceneFromFile(MediaDir + "NivelFisica1\\EscenaSceneEditorFisica1-TgcScene.xml");
            robot = scene.Meshes[178];
            robot.AutoTransform = true;
            /*foreach(TgcMesh m in scene.Meshes)
            {
                m.RotateY(Geometry.DegreeToRadian(90));
                m.updateBoundingBox();
            }*/
            robot.AutoUpdateBoundingBox = true;
            camara_interna = new TgcThirdPersonCamera(robot.BoundingBox.calculateBoxCenter(), robot.BoundingBox.calculateBoxCenter(), 140, 280);
            Camara = camara_interna;
            camara_interna.rotateY(Geometry.DegreeToRadian(180));
        }
        public override void Update()
        {
            PreUpdate();
            var input = Input;
            var movement = TGCVector3.Empty;
            var originalPos = robot.Position;

            movement.X = MovimientoDerecha(input) - MovimientoIzquierda(input);
            movement.Z = MovimientoArriba(input) - MovimientoAbajo(input);
            movement *= ElapsedTime;
            robot.Move(movement);
            robot.BoundingBox.move(movement);

            DetectarColisiones(originalPos);

            vectorCamara.X = robot.Position.X;
            vectorCamara.Y = robot.Position.Y;
            vectorCamara.Z = robot.Position.Z;
            camara_interna.Target = vectorCamara;

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();
            scene.RenderAll();
            foreach (TgcMesh m in scene.Meshes)
            {
                m.BoundingBox.Render();
            }
            PostRender();
        }
        public override void Dispose()
        {
            scene.DisposeAll(); //Dispose de la escena.
        }

        private void DetectarColisiones(TGCVector3 originalPos)
        {
            var collisionFound = false;

            foreach (var mesh in scene.Meshes)
            {
                //Los dos BoundingBox que vamos a testear
                var mainMeshBoundingBox = robot.BoundingBox;
                var sceneMeshBoundingBox = mesh.BoundingBox;

                if (mainMeshBoundingBox == sceneMeshBoundingBox)
                    continue;

                //Ejecutar algoritmo de detección de colisiones
                var collisionResult = TgcCollisionUtils.classifyBoxBox(mainMeshBoundingBox, sceneMeshBoundingBox);

                //Hubo colisión con un objeto. Guardar resultado y abortar loop.
                if (collisionResult != TgcCollisionUtils.BoxBoxResult.Afuera)
                {
                    collisionFound = true;
                    break;
                }
            }

            //Si hubo alguna colisión, entonces restaurar la posición original del mesh
            if (collisionFound)
            {
                robot.Position = originalPos;
            }
        }
        private float MovimientoIzquierda(TgcD3dInput input)
        {
            return MovimientoXZ(input.keyDown(Key.Left) || input.keyDown(Key.A));
        }
        private float MovimientoDerecha(TgcD3dInput input)
        {
            return MovimientoXZ(input.keyDown(Key.Right) || input.keyDown(Key.D));
        }
        private float MovimientoAbajo(TgcD3dInput input)
        {
            return MovimientoXZ(input.keyDown(Key.Down) || input.keyDown(Key.S));
        }
        private float MovimientoArriba(TgcD3dInput input)
        {
            return MovimientoXZ(input.keyDown(Key.Up) || input.keyDown(Key.W));
        }
        private float MovimientoXZ(bool hayMovimiento)
        {
            if (hayMovimiento)
                return MOVEMENT_SPEED;
            return 0;
        }
    }
}