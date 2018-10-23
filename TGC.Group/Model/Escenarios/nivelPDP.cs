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

namespace TGC.Group.Model.Escenarios
{
    class nivelPDP : Escenario
    {

        private TgcScene scene;
        private float velocidadCaminar = 5;
        private float velocidadRotacion = 250;
        private float velocidadDesplazamientoColeccionables = 10f;
        private float direccionDeMovimientoActual = 1;

        private TgcSkeletalMesh personajePrincipal;
        private TgcThirdPersonCamera camaraInterna;

        private float jumping;
        float jump = 0;
        private bool moving = false, enElPiso = true;
        private bool rotating = false;
        private bool techo = false;

        private TgcMesh collider;
        private TgcMesh floorCollider, ceilingCollider;
        private TGCMatrix escalaBase;
        private TGCVector3 lastColliderPos;

        private float incremento = 0f, rotAngle = 0;
        private float distanciaRecorrida = 0f;

        private List<TgcMesh> objectsBehind = new List<TgcMesh>();
        private List<TgcMesh> objectsInFront = new List<TgcMesh>();


        private List<TgcMesh> coleccionables = new List<TgcMesh>();
        private List<TgcMesh> coleccionablesAgarrados = new List<TgcMesh>();
        private int cantidadColeccionables = 0;

        private TgcMp3Player reproductorMp3 = new TgcMp3Player();
        private string pathDeLaCancion;

        private TGCVector3 puerta1 = new TGCVector3(900, 1, 337);
        private TGCVector3 puerta2 = new TGCVector3(1705, 1, 337);
        private TGCVector3 puerta3 = new TGCVector3(3412, 1, 2103);
        private float puertaCruzada = 0;

        // puerta1 = Puerta63
        // puerta2 = Puerta64
        // puerta3 = Puerta85
        // mumukis = Box_4332 - 4340


        /// /////////////////////////////////////////////////////////////////////
        /// ////////////////////////////INIT/////////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////

        public void init(string MediaDir, string shaderDir, TgcCamera camara)
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;
            var loader = new TgcSceneLoader();

            scene = loader.loadSceneFromFile(MediaDir + "ParadigmasEscena\\nivelParadigmas-TgcScene.xml");
            pathDeLaCancion = MediaDir + "Musica\\FeverTime.mp3";


            Console.WriteLine(scene.Meshes[62].Name);
            Console.WriteLine(scene.Meshes[63].Name);

            var skeletalLoader = new TgcSkeletalLoader();
            personajePrincipal = skeletalLoader.loadMeshAndAnimationsFromFile(
                                    MediaDir + "Robot\\Robot-TgcSkeletalMesh.xml",
                                    MediaDir + "Robot\\",
                                    new[]{
                                        MediaDir + "Robot\\Caminando-TgcSkeletalAnim.xml",
                                        MediaDir + "Robot\\Parado-TgcSkeletalAnim.xml",
                                    });

            personajePrincipal.playAnimation("Parado", true);
            personajePrincipal.Position = new TGCVector3(210, 1, 310);
            personajePrincipal.RotateY(Geometry.DegreeToRadian(180));

            camaraInterna = new TgcThirdPersonCamera(personajePrincipal.Position, 250, 500);
            camaraInterna.rotateY(Geometry.DegreeToRadian(180));

            //coleccionables.Add(scene.Meshes[331]);
            //coleccionables.Add(scene.Meshes[332]);
            //coleccionables.Add(scene.Meshes[333]);
            //coleccionables.Add(scene.Meshes[334]);
            //coleccionables.Add(scene.Meshes[335]);
            //coleccionables.Add(scene.Meshes[336]);
            //coleccionables.Add(scene.Meshes[337]);
            //coleccionables.Add(scene.Meshes[338]);
            //coleccionables.Add(scene.Meshes[339]);

            reproductorMp3.FileName = pathDeLaCancion;
            reproductorMp3.play(true);
            AdministradorDeEscenarios.getSingleton().SetCamara(camaraInterna);

        }

        /// /////////////////////////////////////////////////////////////////////
        /// ////////////////////////////UPDATE///////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////


        public void update(float deltaTime, TgcD3dInput input, TgcCamera camara){

            velocidadCaminar = 5;
            if (floorCollider != null) lastColliderPos = floorCollider.Position;

            var moveForward = 0f;
            float rotate = 0;
            moving = false;


            foreach (TgcMesh coleccionable in coleccionables)
            {
                if (!coleccionablesAgarrados.Contains(coleccionable))
                {
                    incremento = velocidadDesplazamientoColeccionables * direccionDeMovimientoActual * deltaTime;
                    coleccionable.Move(0, incremento, 0);
                    distanciaRecorrida = distanciaRecorrida + incremento;
                    if (Math.Abs(distanciaRecorrida) > 1250f)
                    {
                        direccionDeMovimientoActual *= -1;
                    }
                }
            }

            moveForward = MovimientoAbajo(input) - MovimientoArriba(input);
            rotate = RotacionDerecha(input) - RotacionIzquierda(input);
            Salto(input);
            AplicarGravedad(deltaTime);

            if (rotating){
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

        public void render(float deltaTime)
        {

            // reproducirMusica();

            foreach (var mesh in objectsInFront)
            {
                if (!coleccionables.Contains(mesh))
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
                if (!coleccionables.Contains(mesh))
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

                    Slider(lastPos, movementRay);

                    //EstablecerCheckpoint();
                    //MoverObjetos(mesh, movementRay);

                    cruzarPuertas(mesh);

                    personajePrincipal.playAnimation("Caminando", true);
                    coleccionar(mesh);
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


        private void reproducirMusica(TgcD3dInput Input)
        {
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

        private void cruzarPuertas(TgcMesh mesh)
        {
            if (mesh.Name.Contains("Puerta")){
                if (puertaCruzada == 0){
                    personajePrincipal.Position = puerta1;
                    puertaCruzada += 1;
                    return;
                }
                if (puertaCruzada == 1){
                    personajePrincipal.Position = puerta2;
                    puertaCruzada += 1;
                    return;
                }
                if (puertaCruzada == 2){
                    personajePrincipal.Position = puerta3;
                    puertaCruzada += 1;
                    return;
                }

            }
        }

        
        private void coleccionar(TgcMesh mesh)
        {
            if (mesh.Name == "Box_1" && !coleccionables.Contains(mesh))
            {
                coleccionables.Add(mesh);
                cantidadColeccionables++;
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

    }

}
