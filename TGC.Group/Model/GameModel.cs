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
using TGC.Core.SkeletalAnimation;
using System.Collections.Generic;
using TGC.Core.Collision;
using System.Windows.Forms;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer m�s ejemplos chicos, en el caso de copiar para que se
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
        //private TgcMesh robot;
        private TgcSkeletalMesh personajePrincipal;
        private TgcThirdPersonCamera camaraInterna;
        //TGCVector3 vectorCamara = new TGCVector3();
        private List<TgcMesh> meshesDeLaEscena;

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

            //Ac� empieza mi intento de insertar una escena
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "NivelFisica1\\EscenaSceneEditorFisica1-TgcScene.xml");
            //robot = scene.Meshes[178];
            //robot.AutoTransform = true;
            /*foreach(TgcMesh m in scene.Meshes)
            {
                m.RotateY(Geometry.DegreeToRadian(90));
                m.updateBoundingBox();
            }*/
            // robot.AutoUpdateBoundingBox = true;
            //camara_interna = new TgcThirdPersonCamera(robot.BoundingBox.calculateBoxCenter(), robot.BoundingBox.calculateBoxCenter(),  140, 280);

            scene.Meshes[178].Dispose();

            meshesDeLaEscena = new List<TgcMesh>();
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.AutoTransform = true;
                meshesDeLaEscena.Add(mesh);
            }
            var skeletalLoader = new TgcSkeletalLoader();
            personajePrincipal =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "Robot\\Robot-TgcSkeletalMesh.xml",
                    MediaDir + "Robot\\",
                    new[]
                    {
                        MediaDir + "Robot\\Caminando-TgcSkeletalAnim.xml",
                        MediaDir + "Robot\\Parado-TgcSkeletalAnim.xml",
                        MediaDir + "Robot\\Empujar-TgcSkeletalAnim.xml",
                    });
            //Configurar animacion inicial
            personajePrincipal.playAnimation("Parado", true);
            personajePrincipal.AutoTransform = true;
            personajePrincipal.Position = new TGCVector3(400, 0, 400);
            personajePrincipal.RotateY(Geometry.DegreeToRadian(180));
            //Probamos con escala? No sirve
          //  personajePrincipal.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);

            camaraInterna = new TgcThirdPersonCamera(personajePrincipal.BoundingBox.calculateBoxCenter(), 140, 280);
            Camara = camaraInterna;
            camaraInterna.rotateY(Geometry.DegreeToRadian(180));
        }
        public override void Update()
        {
            PreUpdate();
            /*var input = Input;
            var movement = TGCVector3.Empty;
            //var originalPos = robot.Position;

            movement.X = MovimientoDerecha(input) - MovimientoIzquierda(input);
            movement.Z = MovimientoArriba(input) - MovimientoAbajo(input);
            movement *=  ElapsedTime;
            //robot.Move(movement);

            //vectorCamara.X = robot.Position.X;
            vectorCamara.X = personajePrincipal.Position.X;
            //vectorCamara.Y = robot.Position.Y;
            vectorCamara.Y = personajePrincipal.Position.Y;
            //vectorCamara.Z = robot.Position.Z;
            vectorCamara.Z = personajePrincipal.Position.Z;
            //camara_interna.Target = vectorCamara;
            */
            var velocidadCaminar = 400;
            var velocidadRotacion = 250;
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;

            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                rotate = -velocidadRotacion;
                rotating = true;
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                personajePrincipal.RotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                personajePrincipal.playAnimation("Caminando", true);

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                var lastPos = personajePrincipal.Position;

                //La velocidad de movimiento tiene que multiplicarse por el elapsedTime para hacerse independiente de la velocida de CPU
                //Ver Unidad 2: Ciclo acoplado vs ciclo desacoplado

                //NO SE RECOMIENDA UTILIZAR! moveOrientedY mueve el personaje segun la direccion actual, realiza operaciones de seno y coseno.
                personajePrincipal.MoveOrientedY(moveForward * ElapsedTime);

                //Detectar colisiones
                var collide = false;
                //Guardamos los objetos colicionados para luego resolver la respuesta. (para este ejemplo simple es solo 1 caja)
                TgcMesh collider = null;
                foreach (TgcMesh mesh in meshesDeLaEscena)
                {
                    if (TgcCollisionUtils.testAABBAABB(personajePrincipal.BoundingBox, mesh.BoundingBox))
                    {
                        collide = true;
                        collider = mesh;
                        break;
                    }
                }

                //Si hubo colision, restaurar la posicion anterior, CUIDADO!!!!!
                //Hay que tener cuidado con este tipo de respuesta a colision, puede darse el caso que el objeto este parcialmente dentro en este y en el frame anterior.
                //para solucionar el problema que tiene hacer este tipo de respuesta a colisiones y que los elementos no queden pegados hay varios algoritmos y hacks.
                //almacenar la posicion anterior no es lo mejor para responder a una colision.
                //Una primera aproximacion para evitar que haya inconsistencia es realizar sliding
                if (collide)
                {
                    personajePrincipal.Position = lastPos; //Por como esta el framework actualmente esto actualiza el BoundingBox.
                }

                //Si no se esta moviendo, activar animacion de Parado
                else
                {
                    personajePrincipal.playAnimation("Parado", true);
                }

                //Hacer que la camara siga al personaje en su nueva posicion
                camaraInterna.Target = personajePrincipal.Position;

                PostUpdate();
            }
        }

        public override void Render()
        {

            PreRender();

            scene.RenderAll();
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.BoundingBox.Render();
            }

            personajePrincipal.animateAndRender(ElapsedTime);
            personajePrincipal.BoundingBox.Render();

            PostRender();

        }
        public override void Dispose()
        {
            scene.DisposeAll(); //Dispose de la escena.
            personajePrincipal.Dispose(); //Dispose del personaje.

        }
        /*
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
        */
    }
}