using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Core.SkeletalAnimation;
using System.Collections.Generic;
using TGC.Core.Collision;
using System.Reflection;
using System;

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
        private float velocidadDesplazamientolibros = 10f;

        private float direccionDeMovimientoActual = 1;
        private TgcSkeletalMesh personajePrincipal;
        private TgcThirdPersonCamera camaraInterna;
        private List<TgcMesh> meshesDeLaEscena;

        private float jumping;
        private bool moving = false;
        private bool rotating = false;
        private List<TgcMesh> objectsBehind = new List<TgcMesh>();
        private List<TgcMesh> objectsInFront = new List<TgcMesh>();
        private List<TgcMesh> librosAgarrados = new List<TgcMesh>();
        float jump = 0;
        private bool techo = false;
        private TGCMatrix movimientoPlataforma;
        private TgcMesh collider;
        private TgcMesh floorCollider;
        private TGCVector3 lastColliderPos;

        private TgcMesh plataforma1;
        private TgcMesh plataforma2;

        private List<TgcMesh> plataformasMovibles = new List<TgcMesh>();

        private int cantidadDeLibros = 0;

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
        
            meshesDeLaEscena = new List<TgcMesh>();
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.AutoTransform = true;
                meshesDeLaEscena.Add(mesh);
            }

            //Console.WriteLine("el nombre del mesh buscado es: " + scene.Meshes[200].Name);

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
            personajePrincipal.Position = new TGCVector3(400, 1, 400);
            personajePrincipal.Position = new TGCVector3(2400, 1, 1400);
            personajePrincipal.RotateY(Geometry.DegreeToRadian(180));
            //Probamos con escala? No sirve
            //personajePrincipal.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);

            camaraInterna = new TgcThirdPersonCamera(personajePrincipal.BoundingBox.calculateBoxCenter(), 250, 500);
            Camara = camaraInterna;
            camaraInterna.rotateY(Geometry.DegreeToRadian(180));

            plataforma1 = scene.Meshes[164]; //serían la 165 y 166 pero arranca desde 0
            plataforma2 = scene.Meshes[165];
            plataformasMovibles.Add(plataforma1);
            plataformasMovibles.Add(plataforma2);
        }
        public override void Update()
        {
            PreUpdate();
            velocidadCaminar = 5;
            if (floorCollider != null)
                lastColliderPos = floorCollider.Position;
            //Animacion de las plataformas
            plataforma1.Move(0, velocidadDesplazamientoPlataformas * direccionDeMovimientoActual * ElapsedTime, 0);
            if (FastMath.Abs(plataforma1.Position.Y) > 360f)
            {
                direccionDeMovimientoActual *= -1;
            }

            plataforma2.Move(0, velocidadDesplazamientoPlataformas * (-direccionDeMovimientoActual) * ElapsedTime, 0);
            if (FastMath.Abs(plataforma2.Position.Y) > 360f)
            {
                direccionDeMovimientoActual *= -1;
            }

            /*foreach (TgcMesh libro in scene.Meshes) {
                var posicionInicialDelLibro = libro.Position.Y;
                if (libro.Name == "Box_1" && !librosAgarrados.Contains(libro))
                {
                    libro.Move(0, velocidadDesplazamientolibros * direccionDeMovimientoActual * ElapsedTime, 0);         POR AHORA NO LO HAGO, HAY QUE REMODELAR LOS LIBROS DE FISICA 1 PARA QUE TOME CORRECTAMENTE LA ROTACION.
                    if (FastMath.Abs(libro.Position.Y) > 100f || posicionInicialDelLibro == 0)
                    {
                        direccionDeMovimientoActual *= -1;
                    }
                    //libro.RotateY(velocidadRotacionlibros * ElapsedTime);
                }
            }*/
            //Fin de animacion de las plataformas

            var moveForward = 0f;
            float rotate = 0;
            moving = false;

            moveForward = MovimientoAbajo() - MovimientoArriba();
            rotate = RotacionDerecha() - RotacionIzquierda();

            if (floorCollider != null && plataformasMovibles.Contains(floorCollider) && floorCollider.BoundingBox.PMax.Y < personajePrincipal.BoundingBox.PMin.Y)
            {
                TGCVector3 res = floorCollider.Position;
                res.Subtract(lastColliderPos);
                personajePrincipal.Position = personajePrincipal.Position + res;
            }
            Salto();
            AplicarGravedad();


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
            float scale = 1;
            if (!enElPiso())
                scale = 0.4f;
            if (moving)
            {
                //Activar animacion de caminando
                personajePrincipal.playAnimation("Caminando", true);

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                var lastPos = personajePrincipal.Position;
                var pminPersonaje = personajePrincipal.BoundingBox.PMin.Y;
                var pmaxPersonaje = personajePrincipal.BoundingBox.PMax.Y;

                //velocidadCaminar = 5;
                Movimiento = new TGCVector3(FastMath.Sin(personajePrincipal.Rotation.Y) * moveForward, 0, FastMath.Cos(personajePrincipal.Rotation.Y) * moveForward);
                Movimiento.Scale(scale);
                Movimiento.Y = jump;
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
                if (!librosAgarrados.Contains(mesh)) {
                    mesh.Render();
                }                                                   //Aproximacion a solucion de colision con cámara. Habria que mejorar el tema del no renderizado de elementos detras de la misma.
            }

            personajePrincipal.animateAndRender(ElapsedTime);

            PostRender();

        }
        public override void Dispose()
        {

            foreach (TgcMesh mesh in scene.Meshes) {
                if (!librosAgarrados.Contains(mesh)) {
                    mesh.Dispose();
                }
            }
            personajePrincipal.Dispose(); //Dispose del personaje.
            //scene.DisposeAll(); //Dispose de la escena.
        }
        private void DetectarColisionesMovibles(TGCVector3 lastPos, TgcMesh meshAProbar)
        {
            var collisionFound = false;

            foreach (var mesh in scene.Meshes)
            {

                //Los dos BoundingBox que vamos a testear
                var mainMeshBoundingBox = meshAProbar.BoundingBox;
                var sceneMeshBoundingBox = mesh.BoundingBox;

                if (mainMeshBoundingBox == sceneMeshBoundingBox)
                    continue;

                //Ejecutar algoritmo de detección de colisiones
                var collisionResult = TgcCollisionUtils.classifyBoxBox(mainMeshBoundingBox, sceneMeshBoundingBox);

                if (collisionResult != TgcCollisionUtils.BoxBoxResult.Afuera && mainMeshBoundingBox != personajePrincipal.BoundingBox)
                {
                    collisionFound = true;
                }
            }
            if (collisionFound)
            {
                meshAProbar.Position = lastPos;
            }
        }

        private void DetectarColisiones(TGCVector3 lastPos, float pminYAnteriorPersonaje, float pmaxYAnteriorPersonaje)
        {
            var lastCollide = false;
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
                    if (sceneMeshBoundingBox.PMax.Y <= pminYAnteriorPersonaje +10)
                    {
                        lastPos.Y = sceneMeshBoundingBox.PMax.Y + 3;
                        techo = false;
                        floorCollider = mesh;
                    }
                    else if (sceneMeshBoundingBox.PMin.Y > pmaxYAnteriorPersonaje)
                        techo = true;

                    if (floorCollider != null && sceneMeshBoundingBox == floorCollider.BoundingBox)
                        lastCollide = true;

                    collider = mesh;

                    var movementRay = lastPos - personajePrincipal.Position;
                    //Luego debemos clasificar sobre que plano estamos chocando y la direccion de movimiento
                    //Para todos los casos podemos deducir que la normal del plano cancela el movimiento en dicho plano.
                    //Esto quiere decir que podemos cancelar el movimiento en el plano y movernos en el otros.

                    Slider(lastPos, movementRay);

                    MoverObjetos(mesh, movementRay);

                    AgarrarLibros(mesh);
                }
                if (lastCollide == false)
                    floorCollider = null;
            }

        }
        private void Salto()
        {
            if (Input.keyUp(Key.Space) && enElPiso())
            {
                jumping = 2.5f;
                moving = true;

            }
        }
        private bool enElPiso()
        {
            return floorCollider != null && Math.Abs(personajePrincipal.BoundingBox.PMin.Y - floorCollider.BoundingBox.PMax.Y) < 10;
        }
        private void AplicarGravedad()
        {
            if (!enElPiso())
            {
                velocidadCaminar = 1;
                jumping -= 2.5f * ElapsedTime;
                jump = jumping;
                moving = true;
            }
            else
                jump = 0;
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
        private void MoverObjetos(TgcMesh mesh, TGCVector3 movementRay)
        {
            if (mesh.Name == "CajaMadera" && mesh.BoundingBox.PMax.Y >= personajePrincipal.BoundingBox.PMax.Y)
            {
                var lastCajaPos = mesh.Position;
                if (FastMath.Abs(movementRay.X) > FastMath.Abs(movementRay.Z))
                    mesh.Move(5 * Math.Sign(movementRay.X) * -1, 0, 0);
                else
                    mesh.Move(0, 0, 5 * Math.Sign(movementRay.Z) * -1);
                DetectarColisionesMovibles(lastCajaPos, mesh);
            }
        }
        private void AgarrarLibros(TgcMesh mesh)
        {
            if (mesh.Name == "Box_1" && !librosAgarrados.Contains(mesh))
            {
                librosAgarrados.Add(mesh);
                cantidadDeLibros++;
                mesh.BoundingBox = new Core.BoundingVolumes.TgcBoundingAxisAlignBox();
                mesh.Dispose();
            }
        }
        private void Slider(TGCVector3 lastPos, TGCVector3 movementRay)
        {

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

            rs.Scale(0.2f);
            if (!enElPiso())
              rs.Y = -jump;
            personajePrincipal.Position = lastPos - rs;
        }
    }
}