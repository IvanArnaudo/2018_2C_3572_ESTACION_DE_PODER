using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
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
using System.Reflection;
using TGC.Examples.Collision.SphereCollision;

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
        private float velocidadCaminar = 5;
        private float velocidadRotacion = 250;
        private float velocidadDesplazamientoPlataformas = 60f;
        private float direccionDeMovimientoActual1 = 1f;
        private float direccionDeMovimientoActual2 = -1f;
        private TgcSkeletalMesh personajePrincipal;
        private TgcThirdPersonCamera camaraInterna;
        //TGCVector3 vectorCamara = new TGCVector3();
        private List<TgcMesh> meshesDeLaEscena;
        private float jumping;
        private bool moving = false, enElPiso = true;
        private bool rotating = false;
        private readonly List<TgcMesh> objectsBehind = new List<TgcMesh>();
        private readonly List<TgcMesh> objectsInFront = new List<TgcMesh>();
        float jump = 0;
        bool techo = false;
        private SphereCollisionManager collisionManager;
        private TgcMesh collider;

        //private TgcMesh plataforma1, plataforma2;

        // private TgcBoundingSphere characterSphere;

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
            //robot = scene.Meshes[178];
            //robot.AutoTransform = true;
            /*foreach(TgcMesh m in scene.Meshes)
            {
                m.RotateY(Geometry.DegreeToRadian(90));
                m.updateBoundingBox();
            }*/
            // robot.AutoUpdateBoundingBox = true;
            //camara_interna = new TgcThirdPersonCamera(robot.BoundingBox.calculateBoxCenter(), robot.BoundingBox.calculateBoxCenter(),  140, 280);

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
            //personajePrincipal.Position = new TGCVector3(400, 1, 400);
            personajePrincipal.Position = new TGCVector3(1800, -300, 300);
            personajePrincipal.RotateY(Geometry.DegreeToRadian(180));
            //Probamos con escala? No sirve
            //personajePrincipal.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);

            camaraInterna = new TgcThirdPersonCamera(personajePrincipal.BoundingBox.calculateBoxCenter(), 250, 500);
            Camara = camaraInterna;
            camaraInterna.rotateY(Geometry.DegreeToRadian(180));

            //plataforma1 = scene.Meshes[165];
            //plataforma2 = scene.Meshes[166];
        }
        public override void Update()
        {
            PreUpdate();
            /*var originalPos = robot.Position;

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
            //var velocidadCaminar = 400;
            //var velocidadRotacion = 250;


            //Animacion de las plataformas

            scene.Meshes[165].Move(0, velocidadDesplazamientoPlataformas * direccionDeMovimientoActual1 * ElapsedTime, 0);
            if (FastMath.Abs(scene.Meshes[165].Position.Y) >360f)
            {
                direccionDeMovimientoActual1 *= -1;
            }

            scene.Meshes[166].Move(0, velocidadDesplazamientoPlataformas * direccionDeMovimientoActual2 * ElapsedTime, 0);
            if (FastMath.Abs(scene.Meshes[166].Position.Y) > 360f)
            {
                direccionDeMovimientoActual2 *= -1;
            }

            //Fin de animacion de las plataformas

            var moveForward = 0f;
            float rotate = 0;
            moving = false;

            moveForward = MovimientoAbajo() - MovimientoArriba();
            rotate = RotacionDerecha() - RotacionIzquierda();

            if (Input.keyUp(Key.Space) && enElPiso)
            {
                jumping = 2;
                enElPiso = false;
            }
            if (!enElPiso)
            {
                jumping -= 2 * ElapsedTime;
                jump = jumping;
                moving = true;
            }
            else
                jump = 0;

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                personajePrincipal.RotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);
            }

            var Movimiento = TGCVector3.Empty;
            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                personajePrincipal.playAnimation("Caminando", true);

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                var lastPos = personajePrincipal.Position;
                var pminPersonaje = personajePrincipal.BoundingBox.PMin.Y;
                var pmaxPersonaje = personajePrincipal.BoundingBox.PMax.Y;


                Movimiento = new TGCVector3(FastMath.Sin(personajePrincipal.Rotation.Y) * moveForward, jump, FastMath.Cos(personajePrincipal.Rotation.Y) * moveForward);
                personajePrincipal.Move(Movimiento);
                DetectarColisiones(lastPos, pminPersonaje, pmaxPersonaje);

            }else
            {
                personajePrincipal.playAnimation("Parado", true);
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personajePrincipal.Position;

            objectsBehind.Clear();
            objectsInFront.Clear();
            foreach (var mesh in scene.Meshes)
            {
                TGCVector3 colisionCamara;
                if (TgcCollisionUtils.intersectSegmentAABB(camaraInterna.Position, camaraInterna.Target, mesh.BoundingBox, out colisionCamara)) //ACA ESTAMOS GUARDANDO EN UNA LISTA TODOS LOS OBJETOS QUE SE CHOCAN CON LA CAMARA POR DETRAS Y POR ADELANTE.
                {
                    objectsBehind.Add(mesh);
                }
                else
                {
                    objectsInFront.Add(mesh);
                }
            }

            PostUpdate();
        }

        public override void Render()
        {

            PreRender();

            //scene.RenderAll();
            /*foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.BoundingBox.Render();
            }*/

            foreach (var mesh in objectsInFront)
            {
                mesh.Render();                               //Aproximacion a solucion de colision con cámara. Habria que mejorar el tema del no renderizado de elementos detras de la misma.
            }

            personajePrincipal.animateAndRender(ElapsedTime);
            //personajePrincipal.BoundingBox.Render();
            //plataforma1.Render();
            //plataforma2.Render();

            PostRender();

        }
        public override void Dispose()
        {
            scene.DisposeAll(); //Dispose de la escena.
            personajePrincipal.Dispose(); //Dispose del personaje.
            //plataforma1.Dispose();
            //plataforma2.Dispose();

        }

        private void DetectarColisiones(TGCVector3 lastPos, float pminYAnteriorPersonaje, float pmaxYAnteriorPersonaje)
        {
            var collisionFound = false;

            foreach (var mesh in scene.Meshes)
            {
                //Los dos BoundingBox que vamos a testear
                var mainMeshBoundingBox = personajePrincipal.BoundingBox;
                var sceneMeshBoundingBox = mesh.BoundingBox;

                if (mainMeshBoundingBox == sceneMeshBoundingBox)
                    continue;

                //Ejecutar algoritmo de detección de colisiones
                var collisionResult = TgcCollisionUtils.classifyBoxBox(mainMeshBoundingBox, sceneMeshBoundingBox);

                //Hubo colisión con un objeto. Guardar resultado y abortar loop.
                if (collisionResult != TgcCollisionUtils.BoxBoxResult.Afuera)
                {
                    if (sceneMeshBoundingBox.PMax.Y <= pminYAnteriorPersonaje)
                    {
                        enElPiso = true;
                        lastPos.Y = sceneMeshBoundingBox.PMax.Y + 1;
                        techo = false;
                    }
                    else if (sceneMeshBoundingBox.PMin.Y > pmaxYAnteriorPersonaje)
                        techo = true;
                    collisionFound = true;
                    collider = mesh;
                    var movementRay = lastPos - personajePrincipal.Position;
                    //Luego debemos clasificar sobre que plano estamos chocando y la direccion de movimiento
                    //Para todos los casos podemos deducir que la normal del plano cancela el movimiento en dicho plano.
                    //Esto quiere decir que podemos cancelar el movimiento en el plano y movernos en el otros.

                    var rs = TGCVector3.Empty;
                    if (((personajePrincipal.BoundingBox.PMax.X > collider.BoundingBox.PMax.X && movementRay.X > 0) ||
                        (personajePrincipal.BoundingBox.PMin.X < collider.BoundingBox.PMin.X && movementRay.X < 0)) &&
                        ((personajePrincipal.BoundingBox.PMax.Z > collider.BoundingBox.PMax.Z && movementRay.Z > 0) ||
                        (personajePrincipal.BoundingBox.PMin.Z < collider.BoundingBox.PMin.Z && movementRay.Z < 0)))
                    {

                        if (personajePrincipal.Position.X > collider.BoundingBox.PMin.X && personajePrincipal.Position.X < collider.BoundingBox.PMax.X)
                        {
                            rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                        }
                        if (personajePrincipal.Position.Z > collider.BoundingBox.PMin.Z && personajePrincipal.Position.Z < collider.BoundingBox.PMax.Z)
                        {
                            rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                        }

                        //Seria ideal sacar el punto mas proximo al bounding que colisiona y chequear con eso, en ves que con la posicion.
                    }
                    else
                    {
                        if ((personajePrincipal.BoundingBox.PMax.X > collider.BoundingBox.PMax.X && movementRay.X > 0) ||
                            (personajePrincipal.BoundingBox.PMin.X < collider.BoundingBox.PMin.X && movementRay.X < 0))
                        {
                            rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                        }
                        if ((personajePrincipal.BoundingBox.PMax.Z > collider.BoundingBox.PMax.Z && movementRay.Z > 0) ||
                            (personajePrincipal.BoundingBox.PMin.Z < collider.BoundingBox.PMin.Z && movementRay.Z < 0))
                        {
                            rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                        }
                    }
                    personajePrincipal.Position = lastPos - rs;
                    //break;
                }
            }
            if (collisionFound)
            {
                /*if (!enElPiso)
                {
                    if (techo)
                        jump = -2 * ElapsedTime;
                    lastPos.Y += jump;
                }
                personajePrincipal.Position = lastPos;*/
                
            }
            else
                enElPiso = false;
        }
        private float RotacionIzquierda()
        {
            return Movimiento(Input.keyDown(Key.Left) || Input.keyDown(Key.A), "Rotacion");
        }
        private float RotacionDerecha()
        {
            return Movimiento(Input.keyDown(Key.Right) || Input.keyDown(Key.D), "Rotacion");
        }
        private float MovimientoAbajo()
        {
            return Movimiento(Input.keyDown(Key.Down) || Input.keyDown(Key.S), "Caminar");
        }
        private float MovimientoArriba()
        {
            return Movimiento(Input.keyDown(Key.Up) || Input.keyDown(Key.W), "Caminar");
        }
        private float Rotacion()
        {
            rotating = true;
            return velocidadRotacion;
        }
        private float Caminar()
        {
            moving = true;
            return velocidadCaminar;
        }
        private float Movimiento(bool hayMovimiento, string tipoMovimiento)
        {
            if (hayMovimiento)
               return CallFloatMethod(tipoMovimiento);
            return 0;
        }
        private float CallFloatMethod(string methodName)
        {
            return (float)this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, null);
        }
    }
}