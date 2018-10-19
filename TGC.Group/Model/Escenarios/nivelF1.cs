using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using System.Collections.Generic;
using TGC.Core.Collision;
using System.Reflection;
using System;
using TGC.Core.Sound;
using TGC.Core.Input;
using TGC.Core.Camara;
using TGC.Core.BoundingVolumes;
using System.Drawing;

namespace TGC.Group.Model.Escenarios
{
    class nivelF1:Escenario{

        private float velocidadCaminar = 5;
        private float velocidadRotacion = 250;
        private float velocidadDesplazamientoPlataformas = 60f;
        private float velocidadDesplazamientolibros = 10f;

        private float direccionDeMovimientoActual = 1;
        private TgcSkeletalMesh personajePrincipal;
        private TgcThirdPersonCamera camaraInterna;
        private List<TgcMesh> meshesDeLaEscena;

        private float jumping;
        private bool moving = false, enElPiso = true;
        private bool rotating = false;
        private List<TgcMesh> objectsBehind = new List<TgcMesh>();
        private List<TgcMesh> objectsInFront = new List<TgcMesh>();
        private List<TgcMesh> librosAgarrados = new List<TgcMesh>();
        float jump = 0;
        private bool techo = false;
        private TGCMatrix movimientoPlataforma;
        private TgcMesh collider;
        private TgcMesh floorCollider, ceilingCollider;
        private TGCMatrix escalaBase;

        private TGCVector3 lastColliderPos;

        private TgcMesh plataforma1;
        private TgcMesh plataforma2;

        private TgcMp3Player reproductorMp3 = new TgcMp3Player();

        private string pathDeLaCancion;

        private List<TgcMesh> plataformasMovibles = new List<TgcMesh>();

        private int cantidadDeLibros = 0;

        private TgcScene scene;

        private TGCVector3 puntoCheckpointActual = new TGCVector3(409, 151, 337);
        private TGCVector3 puntoCheckpoint1 = new TGCVector3(450, 400, 2570);

        private float incremento = 0f, rotAngle = 0;
        private float distanciaRecorrida = 0f;

        private TgcBoundingAxisAlignBox checkpoint1 = new TgcBoundingAxisAlignBox(new TGCVector3(835, 386, 2561), new TGCVector3(71, 571, 2591));
        /// /////////////////////////////////////////////////////////////////////
        /// ////////////////////////////INIT/////////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////


        public void init(string MediaDir, string shaderDir, TgcCamera camara)
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "NivelFisica1\\EscenaSceneEditorFisica1-TgcScene.xml");

            pathDeLaCancion = MediaDir + "Musica\\FeverTime.mp3";

            meshesDeLaEscena = new List<TgcMesh>();

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
            
            personajePrincipal.Position = new TGCVector3(400, 1, 400);
           // personajePrincipal.Position = new TGCVector3(2400, 1, 1400);
            personajePrincipal.RotateY(Geometry.DegreeToRadian(180));


            camaraInterna = new TgcThirdPersonCamera(personajePrincipal.Position, 250, 500);
         // camara = camaraInterna;
            camaraInterna.rotateY(Geometry.DegreeToRadian(180));

            plataforma1 = scene.Meshes[164]; //serían la 165 y 166 pero arranca desde 0
            plataforma2 = scene.Meshes[165];
            plataformasMovibles.Add(plataforma1);
            plataformasMovibles.Add(plataforma2);

            reproductorMp3.FileName = pathDeLaCancion;
            //reproductorMp3.play(true);
            AdministradorDeEscenarios.getSingleton().SetCamara(camaraInterna);
           
        }

        /// /////////////////////////////////////////////////////////////////////
        /// ////////////////////////////UPDATE///////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////


        public void update(float deltaTime, TgcD3dInput input, TgcCamera camara){

            //AdministradorDeEscenarios.getSingleton().SetCamara(camaraInterna);

            velocidadCaminar = 5;
            if (floorCollider != null)
                lastColliderPos = floorCollider.Position;

            // Animacion de las plataformas

            plataforma1.Move(0, velocidadDesplazamientoPlataformas * direccionDeMovimientoActual * deltaTime, 0);
            if (FastMath.Abs(plataforma1.Position.Y) > 360f)
            {
                direccionDeMovimientoActual *= -1;
            }

            plataforma2.Move(0, velocidadDesplazamientoPlataformas * (-direccionDeMovimientoActual) * deltaTime, 0);
            if (FastMath.Abs(plataforma2.Position.Y) > 360f)
            {
                direccionDeMovimientoActual *= -1;
            }

            //Animacion de los libros de F1

            foreach (TgcMesh libro in scene.Meshes)
            {
                if (libro.Name == "Box_1" && !librosAgarrados.Contains(libro))
                {
                    incremento = velocidadDesplazamientolibros * direccionDeMovimientoActual * deltaTime;
                    libro.Move(0, incremento, 0);
                    distanciaRecorrida = distanciaRecorrida + incremento;
                    if (Math.Abs(distanciaRecorrida) > 1250f)
                    {
                        direccionDeMovimientoActual *= -1;
                    }
                }
            }


            var moveForward = 0f;
            float rotate = 0;
            moving = false;

            moveForward = MovimientoAbajo(input) - MovimientoArriba(input);
            rotate = RotacionDerecha(input) - RotacionIzquierda(input);

           

            if (floorCollider != null && plataformasMovibles.Contains(floorCollider) && floorCollider.BoundingBox.PMax.Y < personajePrincipal.BoundingBox.PMin.Y)
            {
                TGCVector3 res = floorCollider.Position;
                res.Subtract(lastColliderPos);
                personajePrincipal.Position = personajePrincipal.Position + res;
            }
            Salto(input);
            AplicarGravedad(deltaTime);


            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                rotAngle = Geometry.DegreeToRadian(rotate * deltaTime);
                personajePrincipal.RotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);

            }

            var Movimiento = TGCVector3.Empty;
            //Si hubo desplazamiento
            float scale = 1;
            if (!enElPiso)
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
                

            }
            else
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
            var Rot = TGCMatrix.RotationY(personajePrincipal.Rotation.Y);
            var T = TGCMatrix.Translation(personajePrincipal.Position);
            escalaBase = Rot * T;
            personajePrincipal.Transform = escalaBase;
        }


        /////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////RENDER///////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////

        public void render(float deltaTime){

           // reproducirMusica();

            foreach (var mesh in objectsInFront)
            {
                if (!librosAgarrados.Contains(mesh))
                {
                //    var resultadoColisionFrustum = TgcCollisionUtils.classifyFrustumAABB(Frustum, mesh.BoundingBox);
                //    if (resultadoColisionFrustum != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        mesh.Render();
                } 
           //Aproximacion a solucion de colision con cámara. Habria que mejorar el tema del no renderizado de elementos detras de la misma.
            }

            personajePrincipal.animateAndRender(deltaTime);

        }


        /////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////DISPOSE//////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////

        public void dispose()
        {

            foreach (TgcMesh mesh in scene.Meshes)
            {
                if (!librosAgarrados.Contains(mesh))
                {
                    mesh.Dispose();
                }
            }
            personajePrincipal.Dispose(); //Dispose del personaje.
            //scene.DisposeAll(); //Dispose de la escena.

            reproductorMp3.closeFile();
        }

        /////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////MISC/////////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////

        private bool DistanciaAlPisoSalto()
        {
            return floorCollider != null && Math.Abs(personajePrincipal.BoundingBox.PMin.Y - floorCollider.BoundingBox.PMax.Y) < 10;
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

        private void detectarSiHayColisionDeCheckpoints(TGCVector3 lastPos, float pminYAnteriorPersonaje, float pmaxYAnteriorPersonaje)
        {
            var mainMeshBoundingBox = personajePrincipal.BoundingBox;
            //El checkpoint 1 fue atravesado
            if (TgcCollisionUtils.testAABBAABB(mainMeshBoundingBox, checkpoint1)){
                puntoCheckpointActual = puntoCheckpoint1;
            }

        }



        private void DetectarColisiones(TGCVector3 lastPos, float pminYAnteriorPersonaje, float pmaxYAnteriorPersonaje)
        {
            var lastCollide = false;
            detectarSiHayColisionDeCheckpoints(lastPos, pminYAnteriorPersonaje, pmaxYAnteriorPersonaje);

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
                    if (sceneMeshBoundingBox.PMax.Y <= pminYAnteriorPersonaje + 10)
                    {
                        enElPiso = true;
                        lastPos.Y = sceneMeshBoundingBox.PMax.Y + 3;
                        floorCollider = mesh;
                    }
                    else if (sceneMeshBoundingBox.PMin.Y > pmaxYAnteriorPersonaje && jump != 0)
                    {
                        ceilingCollider = mesh;
                        techo = true;
                    }

                    if (floorCollider != null && sceneMeshBoundingBox == floorCollider.BoundingBox)
                        lastCollide = true;


                    collider = mesh;

                    var movementRay = lastPos - personajePrincipal.Position;
                    //Luego debemos clasificar sobre que plano estamos chocando y la direccion de movimiento
                    //Para todos los casos podemos deducir que la normal del plano cancela el movimiento en dicho plano.
                    //Esto quiere decir que podemos cancelar el movimiento en el plano y movernos en el otros.

                    Slider(lastPos, movementRay);
               //     EstablecerCheckpoint();
                    MoverObjetos(mesh, movementRay);
                    CaerseAlAgua(mesh,movementRay);
                    personajePrincipal.playAnimation("Caminando", true);
                    AgarrarLibros(mesh);
                }
                if (lastCollide == false)
                {
                    enElPiso = false;
                    //floorCollider = null;
                }

            }

        }


        private void Salto(TgcD3dInput input)
        {
            if (input.keyUp(Key.Space) && DistanciaAlPisoSalto())
            {
                jumping = 2.5f;
                moving = true;
                enElPiso = false;
            }
        }

        private void AplicarGravedad(float dTime)
        {
            if (!enElPiso)
            {
                velocidadCaminar = 1;
                jumping -= 2.5f * dTime;
                jump = jumping;
                moving = true;
            }
            else
                jump = 0;
        }
        private float RotacionIzquierda(TgcD3dInput Input)
        {
            return Movimiento(Input.keyDown(Key.Left) || Input.keyDown(Key.A), "Rotacion");
        }
        private float RotacionDerecha(TgcD3dInput Input)
        {
            return Movimiento(Input.keyDown(Key.Right) || Input.keyDown(Key.D), "Rotacion");
        }
        private float MovimientoAbajo(TgcD3dInput Input)
        {
            return Movimiento(Input.keyDown(Key.Down) || Input.keyDown(Key.S), "Caminar");
        }
        private float MovimientoArriba(TgcD3dInput Input)
        {
            return Movimiento(Input.keyDown(Key.Up) || Input.keyDown(Key.W), "Caminar");
        }
        private void reproducirMusica(TgcD3dInput Input){
            var estadoActual = reproductorMp3.getStatus();
            if (Input.keyPressed(Key.M))
            {
                if (estadoActual == TgcMp3Player.States.Open)
                {
                    //Reproducir MP3
                    reproductorMp3.play(true);
                }
                if (estadoActual == TgcMp3Player.States.Stopped)
                {
                    //Parar y reproducir MP3
                    reproductorMp3.closeFile();
                    reproductorMp3.play(true);
                }
                if (estadoActual == TgcMp3Player.States.Playing)
                {
                    //Parar el MP3
                    reproductorMp3.stop();
                }
            }
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
                {
                    personajePrincipal.playAnimation("Empujar", true);
                    mesh.Move(5 * Math.Sign(movementRay.X) * -1, 0, 0);
                    DetectarColisionesMovibles(lastCajaPos, mesh);
                }
                else
                 if (!(FastMath.Abs(movementRay.X) > FastMath.Abs(movementRay.Z)))
                {
                    personajePrincipal.playAnimation("Empujar", true);
                    mesh.Move(0, 0, 5 * Math.Sign(movementRay.Z) * -1);
                    DetectarColisionesMovibles(lastCajaPos, mesh);
                }
            }
        }

        private void CaerseAlAgua(TgcMesh mesh, TGCVector3 movementRay)
        {
            if (mesh.Name.Contains("Agua") && mesh.Name.Contains("Floor"))
            {
                personajePrincipal.Position = puntoCheckpointActual;
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
            if (!enElPiso && !techo)
                rs.Y = -jump;
            else if (techo)
            {
                rs.Y = Math.Abs(personajePrincipal.BoundingBox.PMax.Y - ceilingCollider.BoundingBox.PMax.Y);
                techo = false;
            }
            personajePrincipal.Position = lastPos - rs;
        }

       

        /*private void cargarCancion(string direccionDeArchivo)
        {
            if (archivoActual == null || archivoActual != direccionDeArchivo)
            {
                archivoActual = direccionDeArchivo;                                     Esto es para cargar a otra cancion en el transcurso del juego, lo dejo aca por si interesa en un futuro.

                //Cargar archivo de la cancion
                reproductorMp3.closeFile();
                reproductorMp3.FileName = archivoActual;
            }
        }*/
        



    }

}

