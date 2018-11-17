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
using TGC.Core.Textures;
using TGC.Group.Model.Interfaz;
using Microsoft.DirectX;
using TGC.Core.Shaders;
using TGC.Core.Particle;

namespace TGC.Group.Model.Escenarios
{
    class nivelF1:Escenario{

        private float resolucionX;
        private float resolucionY;

        private float velocidadCaminar = 3;
        private float velocidadRotacion = 250;
        private float velocidadDesplazamientoPlataformas = 100f;
        private float velocidadDesplazamientolibros = 50f;
        private float velocidadDesplazamientoBolasDeCanion = 200f;
        private float sliderModifier = 1;
        private string sliderModifierType = "none";
        List<TgcMesh> slowSliders = new List<TgcMesh>();
        List<TgcMesh> fastSliders = new List<TgcMesh>();
        List<TgcMesh> aguas = new List<TgcMesh>();


        private float tiempoOlas;
        private float signo = 1;

        private float direccionDeMovimientoActualLibrosF1 = 1;
        private float direccionDeMovimientoActualPlataforma1 = 1;
        private float direccionDeMovimientoActualPlataforma2 = 1;

        private TgcSkeletalMesh personajePrincipal;
        private TgcThirdPersonCamera camaraInterna;
        private List<TgcMesh> meshesDeLaEscena;

        private float jumping;
        private bool moving = false, enElPiso = true;
        private bool rotating = false;
        private List<TgcMesh> objectsBehind = new List<TgcMesh>();
        private List<TgcMesh> objectsInFront = new List<TgcMesh>();
        private List<TgcMesh> librosAgarrados = new List<TgcMesh>();
        private Boton librosAdquiridos;
        private float cantidadLibrosAdquiridos;
        float jump = 0;
        private bool techo = false;
        private TgcMesh collider;
        private TgcMesh floorCollider, ceilingCollider, sliderFloorCollider;
        private TGCMatrix escalaBase;

        private TGCVector3 lastColliderPos;

        private TgcMesh plataforma1;
        private TgcMesh plataforma2;

        TGCVector3 centroPlataforma1 = new TGCVector3(2520, -195, 585);
        TGCVector3 centroPlataforma2 = new TGCVector3(2520, -195, 305);

        private TgcMesh bolaDeCanion1;
        private TgcMesh bolaDeCanion2;
        private TgcMesh bolaDeCanion3;

        TGCVector3 posicionInicialBolaDeCanion1 = new TGCVector3();
        TGCVector3 posicionInicialBolaDeCanion2 = new TGCVector3();
        TGCVector3 posicionInicialBolaDeCanion3 = new TGCVector3();

        TGCVector3 posicionInicialEmisorDeParticulas1 = new TGCVector3();
        TGCVector3 posicionInicialEmisorDeParticulas2 = new TGCVector3();
        TGCVector3 posicionInicialEmisorDeParticulas3 = new TGCVector3();

        private TgcMp3Player reproductorMp3 = new TgcMp3Player();

        private string pathDeLaCancion;

        private List<TgcMesh> plataformasMovibles = new List<TgcMesh>();
        private List<TgcMesh> bolasDeCanion = new List<TgcMesh>();

        private TgcScene scene;

        private TGCVector3 puntoCheckpointActual = new TGCVector3(400, 1, 400);
        //private TGCVector3 puntoCheckpointActual = new TGCVector3(1500, -590, 1500);
        //private TGCVector3 puntoCheckpointActual = new TGCVector3(2392, 61, 3308);

        private TGCVector3 puntoCheckpoint1 = new TGCVector3(410, 322, 5050);
        private TGCVector3 puntoCheckpoint2 = new TGCVector3(1129, -567, 155);
        private List<TgcMesh> lights = new List<TgcMesh>();

        private float incremento = 0f, incrementoBola1 = 0f, incrementoBola2 = 0f, incrementoBola3 = 0f, rotAngle = 0f;
        private float distanciaRecorrida = 0f;
        private float distanciaRecorridaBola1 = 0f;
        private float distanciaRecorridaBola2 = 0f;
        private float distanciaRecorridaBola3 = 0f;
        private float cantVidas;
        private Sprite HUD;
        private TgcTexture vida;
        private TgcTexture fisicaLib;
        private TgcBoundingAxisAlignBox checkpoint1 = new TgcBoundingAxisAlignBox(new TGCVector3(839, 591, 4969), new TGCVector3(23, 395, 5120));
        private TgcBoundingAxisAlignBox checkpoint2 = new TgcBoundingAxisAlignBox(new TGCVector3(1621, -68, 7766), new TGCVector3(923, -565, 8069));
        private int posVidas;
        private Microsoft.DirectX.Direct3D.Effect effect;
        private Microsoft.DirectX.Direct3D.Effect efectoOlas;


        private ParticleEmitter emisorDeParticulas1;
        private ParticleEmitter emisorDeParticulas2;
        private ParticleEmitter emisorDeParticulas3;
        private int cantidadDeParticulas;
        private string pathTexturaEmisorDeParticulas;

        /////////////////////////////////////////////////////////////////////////
        ////////////////////////////////INIT/////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////


        public void init(string MediaDir, string shaderDir, TgcCamera camara)
        {
            var d3dDevice = D3DDevice.Instance.Device;
            resolucionX = d3dDevice.PresentationParameters.BackBufferWidth;
            resolucionY = d3dDevice.PresentationParameters.BackBufferHeight;

            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();

            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "NivelFisica1\\EscenaSceneEditorFisica1-TgcScene.xml");

            SetearAguas();

            pathDeLaCancion = MediaDir + "Musica\\FeverTime.mp3";


            meshesDeLaEscena = new List<TgcMesh>();

            HUD = new Sprite(D3DDevice.Instance.Device);
            vida = TgcTexture.createTexture(MediaDir + "Textures\\vida.png");
            fisicaLib = TgcTexture.createTexture(MediaDir + "NivelFisica1\\Textures\\TexturaTapaLibro.jpg");

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

            personajePrincipal.Position = puntoCheckpointActual;
            personajePrincipal.RotateY(Geometry.DegreeToRadian(180));


            camaraInterna = new TgcThirdPersonCamera(personajePrincipal.Position, 250, 500);
            // camara = camaraInterna;
            camaraInterna.rotateY(Geometry.DegreeToRadian(180));

            librosAdquiridos = new Boton(cantidadLibrosAdquiridos.ToString(), 0.925f, 0.88f, null);

            plataforma1 = scene.Meshes[164];
            plataforma2 = scene.Meshes[165];

            plataformasMovibles.Add(plataforma1);
            plataformasMovibles.Add(plataforma2);

            bolaDeCanion1 = scene.Meshes[172];
            bolaDeCanion2 = scene.Meshes[173];
            bolaDeCanion3 = scene.Meshes[174];

            posicionInicialBolaDeCanion1 = scene.Meshes[172].Position;
            posicionInicialBolaDeCanion2 = scene.Meshes[173].Position;
            posicionInicialBolaDeCanion3 = scene.Meshes[174].Position;

            bolasDeCanion.Add(bolaDeCanion1);
            bolasDeCanion.Add(bolaDeCanion2);
            bolasDeCanion.Add(bolaDeCanion3);

            pathTexturaEmisorDeParticulas = MediaDir + "Textures\\fuego.png";
            cantidadDeParticulas = 10;

            emisorDeParticulas1 = new ParticleEmitter(pathTexturaEmisorDeParticulas, cantidadDeParticulas);
            emisorDeParticulas1.MinSizeParticle = 30f;
            emisorDeParticulas1.MaxSizeParticle = 30f;
            emisorDeParticulas1.ParticleTimeToLive = 1f;
            emisorDeParticulas1.CreationFrecuency = 0.25f;
            emisorDeParticulas1.Dispersion = 500;
            emisorDeParticulas1.Speed = new TGCVector3(-25, 40, 50);
            posicionInicialEmisorDeParticulas1 = new TGCVector3(1935, 200, 4345);
            emisorDeParticulas1.Position = posicionInicialEmisorDeParticulas1;

            emisorDeParticulas2 = new ParticleEmitter(pathTexturaEmisorDeParticulas, cantidadDeParticulas);
            emisorDeParticulas2 = new ParticleEmitter(pathTexturaEmisorDeParticulas, cantidadDeParticulas);
            emisorDeParticulas2.MinSizeParticle = 30f;
            emisorDeParticulas2.MaxSizeParticle = 30f;
            emisorDeParticulas2.ParticleTimeToLive = 1f;
            emisorDeParticulas2.CreationFrecuency = 0.25f;
            emisorDeParticulas2.Dispersion = 500;
            emisorDeParticulas2.Speed = new TGCVector3(-25, 40, 50);
            posicionInicialEmisorDeParticulas2 = new TGCVector3(2205, 200, 4345);
            emisorDeParticulas2.Position = posicionInicialEmisorDeParticulas2;

            emisorDeParticulas3 = new ParticleEmitter(pathTexturaEmisorDeParticulas, cantidadDeParticulas);
            emisorDeParticulas3 = new ParticleEmitter(pathTexturaEmisorDeParticulas, cantidadDeParticulas);
            emisorDeParticulas3.MinSizeParticle = 30f;
            emisorDeParticulas3.MaxSizeParticle = 30f;
            emisorDeParticulas3.ParticleTimeToLive = 1f;
            emisorDeParticulas3.CreationFrecuency = 0.25f;
            emisorDeParticulas3.Dispersion = 500;
            emisorDeParticulas3.Speed = new TGCVector3(-25, 40, 50);
            posicionInicialEmisorDeParticulas3 = new TGCVector3(2495, 200, 4345);
            emisorDeParticulas3.Position = posicionInicialEmisorDeParticulas3;

            reproductorMp3.FileName = pathDeLaCancion;
            reproductorMp3.play(true);

            AdministradorDeEscenarios.getSingleton().SetCamara(camaraInterna);

            cantVidas = 3;
            effect = TgcShaders.loadEffect(shaderDir + "MultiDiffuseLights.fx");
            for (var i = 224; i < 250; ++i)
                lights.Add(scene.Meshes[i]);

            scene.Meshes[4].D3dMesh.ComputeNormals();
            scene.Meshes[48].D3dMesh.ComputeNormals();
            var lightColors = new ColorValue[lights.Count];
            var pointLightPositions = new Vector4[lights.Count];
            var pointLightIntensity = new float[lights.Count];
            var pointLightAttenuation = new float[lights.Count];
            for (var i = 0; i < lights.Count; i++)
            {
                var lightMesh = lights[i];

                lightColors[i] = ColorValue.FromColor(Color.White);
                pointLightPositions[i] = TGCVector3.Vector3ToVector4(lightMesh.BoundingBox.Position);
                pointLightIntensity[i] = 20;
                pointLightAttenuation[i] = 0.07f;
            }
            foreach (var mesh in scene.Meshes)
            {
                if (mesh.Name.Contains("Box") || mesh.Name.Contains("Madera") || mesh.Name.Contains("East") || mesh.Name.Contains("South") || mesh.Name.Contains("North") || mesh.Name.Contains("West"))
                    continue;
                mesh.Effect = effect;

                mesh.Effect.SetValue("lightColor", lightColors);
                mesh.Effect.SetValue("lightPosition", pointLightPositions);
                mesh.Effect.SetValue("lightIntensity", pointLightIntensity);
                mesh.Effect.SetValue("lightAttenuation", pointLightAttenuation);
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                mesh.Technique = "MultiDiffuseLightsTechnique";
                if (mesh.Name.Contains("Box") || mesh.Name.Contains("Madera") )
                {
                    mesh.Effect.SetValue("lightAttenuation", pointLightAttenuation);
                    mesh.D3dMesh.ComputeNormals();
                }
            }

            AplicarShaders(shaderDir);

        }

        /////////////////////////////////////////////////////////////////////////
        ////////////////////////////////UPDATE///////////////////////////////////
        /////////////////////////////////////////////////////////////////////////


        public void update(float deltaTime, TgcD3dInput input, TgcCamera camara){

            reproducirMusica(input);

            velocidadCaminar = 350 * deltaTime;

            if (floorCollider != null)
            {
                lastColliderPos = floorCollider.Position;
            }

            if (Math.Abs(tiempoOlas) > 5) cambiarSigno();

            if (signo < 0) tiempoOlas -= deltaTime;
            if (signo > 0) tiempoOlas += deltaTime;

            animarLibrosF1(deltaTime);
            animarPlataformas(deltaTime);
            animarBolasDeCanionYEmisoresDeParticulas(deltaTime);
            ChocarConBolasDeCanion();

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

            if (rotating)
            {
                rotAngle = Geometry.DegreeToRadian(rotate * deltaTime);
                personajePrincipal.RotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);

            }

            var Movimiento = TGCVector3.Empty;
            //Si hubo desplazamiento
            float scale = 1;
            if (!enElPiso)
                scale = 0.7f;
            if (moving)
            {
                personajePrincipal.playAnimation("Caminando", true);

                var lastPos = personajePrincipal.Position;
                var pminPersonaje = personajePrincipal.BoundingBox.PMin.Y;
                var pmaxPersonaje = personajePrincipal.BoundingBox.PMax.Y;

                Movimiento = new TGCVector3(FastMath.Sin(personajePrincipal.Rotation.Y) * moveForward, 0, FastMath.Cos(personajePrincipal.Rotation.Y) * moveForward);
                Movimiento.Scale(scale * sliderModifier);
                Movimiento.Y = jump * deltaTime;
                personajePrincipal.Move(Movimiento);
                
                DetectarColisiones(lastPos, pminPersonaje, pmaxPersonaje, deltaTime);
                

            }
            else
            {
                personajePrincipal.playAnimation("Parado", true);
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personajePrincipal.Position;

            
            ajustarPosicionDeCamara();
            var Rot = TGCMatrix.RotationY(personajePrincipal.Rotation.Y);
            var T = TGCMatrix.Translation(personajePrincipal.Position);
            escalaBase = Rot * T;
            personajePrincipal.Transform = escalaBase;


        }


        /////////////////////////////////////////////////////////////////////////
        ////////////////////////////////RENDER///////////////////////////////////
        /////////////////////////////////////////////////////////////////////////

        public void render(float deltaTime, TgcFrustum frustum){

            
            foreach (var mesh in objectsInFront)
            {
               
                if (!librosAgarrados.Contains(mesh))
                {
                    var resultadoColisionFrustum = TgcCollisionUtils.classifyFrustumAABB(frustum, mesh.BoundingBox);
                    if (resultadoColisionFrustum != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        mesh.Render();
                } 
            }

            efectoOlas.SetValue("time", tiempoOlas);

            personajePrincipal.animateAndRender(deltaTime);

            HUD.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);

            posVidas = D3DDevice.Instance.Device.Viewport.Width - vida.Width;

            for (int i = 0; i < cantVidas; i++)
            {
                HUD.Transform = TGCMatrix.Translation(new TGCVector3(posVidas, 0, 0));
                HUD.Draw(vida.D3dTexture, Rectangle.Empty, Vector3.Empty, Vector3.Empty, Color.OrangeRed);
                posVidas -= vida.Width;
            }

            librosAdquiridos.cambiarTexto(cantidadLibrosAdquiridos.ToString());
            librosAdquiridos.Render();

            HUD.Draw2D(fisicaLib.D3dTexture, Rectangle.Empty, new SizeF(50, 50), new PointF(D3DDevice.Instance.Width - 50, D3DDevice.Instance.Height - 90), Color.White);
            HUD.End();

            emisorDeParticulas1.render(deltaTime);
            emisorDeParticulas2.render(deltaTime);
            emisorDeParticulas3.render(deltaTime);
        }


        /////////////////////////////////////////////////////////////////////////
        ////////////////////////////////DISPOSE//////////////////////////////////
        /////////////////////////////////////////////////////////////////////////

        public void dispose()
        {

            foreach (TgcMesh mesh in scene.Meshes)
            {              
                if (!librosAgarrados.Contains(mesh))
                {
                    mesh.Dispose();
                }
            }

            personajePrincipal.Dispose();

            reproductorMp3.closeFile();

            emisorDeParticulas1.dispose();
            emisorDeParticulas2.dispose();
            emisorDeParticulas3.dispose();
        }

        /////////////////////////////////////////////////////////////////////////
        ////////////////////////////////FUNCIONES////////////////////////////////
        /////////////////////////////////////////////////////////////////////////

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
            var colisionCheckp = TgcCollisionUtils.classifyBoxBox(mainMeshBoundingBox, checkpoint1);
            var colisionCheckp2 = TgcCollisionUtils.classifyBoxBox(mainMeshBoundingBox, checkpoint2);
            if (colisionCheckp != TgcCollisionUtils.BoxBoxResult.Afuera)
            {
                puntoCheckpointActual = puntoCheckpoint1;
            } else if (colisionCheckp2 != TgcCollisionUtils.BoxBoxResult.Afuera)
            {
                puntoCheckpointActual = puntoCheckpoint2;
            }
        }



        private void DetectarColisiones(TGCVector3 lastPos, float pminYAnteriorPersonaje, float pmaxYAnteriorPersonaje, float dtime)
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
                        jump = 0;
                        jumping = 0;
                        enElPiso = true;
                        lastPos.Y = sceneMeshBoundingBox.PMax.Y + 3;
                        floorCollider = mesh;
                        if (slowSliders.Contains(mesh))
                        {
                            sliderFloorCollider = mesh;
                            sliderModifier = 0.2f;
                        }
                        else if (fastSliders.Contains(mesh))
                        {
                            sliderFloorCollider = mesh;
                            sliderModifier = 3;
                        }
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

                    Slider(lastPos, movementRay, dtime);
                    MoverObjetos(mesh, movementRay);
                    CaerseAlAgua(mesh,movementRay);
                    verSiSeCompletoNivel(mesh);
                    personajePrincipal.playAnimation("Caminando", true);
                    AgarrarLibros(mesh);
                }
                if (lastCollide == false && floorCollider != null)
                {
                    personajePrincipal.Move(0, -3, 0);
                    if (TgcCollisionUtils.classifyBoxBox(personajePrincipal.BoundingBox, floorCollider.BoundingBox) == TgcCollisionUtils.BoxBoxResult.Afuera)
                        enElPiso = false;
                    personajePrincipal.Move(0, 3, 0);
                }
                else if (floorCollider == null)
                    enElPiso = false;

            }

        }


        private void Salto(TgcD3dInput input)
        {
            if (input.keyUp(Key.Space) && DistanciaAlPisoSalto())
            {
                jumping = 280f;
                moving = true;
                enElPiso = false;
            }
        }

        private void AplicarGravedad(float dTime)
        {
            if (!enElPiso)
            {
                velocidadCaminar = 750 * dTime;
                jumping -= 300 * dTime;
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

            if ((mesh.Name.Contains("Agua") && mesh.Name.Contains("Floor")) || mesh.Name == "Subsuelo9-Floor-0")
            {
                if (cantVidas < 1)
                {
                    AdministradorDeEscenarios.getSingleton().agregarEscenario(new GameOver(), camaraInterna);
                }
                personajePrincipal.Position = puntoCheckpointActual;
                cantVidas--;
            
            }
            
        }

        private void ChocarConBolasDeCanion()
        {
            foreach (var mesh in scene.Meshes)
            {

                var mainMeshBoundingBox = personajePrincipal.BoundingBox;
                var sceneMeshBoundingBox = mesh.BoundingBox;

                if (mainMeshBoundingBox == sceneMeshBoundingBox)
                    continue;

                var collisionResult = TgcCollisionUtils.classifyBoxBox(mainMeshBoundingBox, sceneMeshBoundingBox);

                if (collisionResult != TgcCollisionUtils.BoxBoxResult.Afuera)
                {
                    if (mesh.Name == "Sphere")
                    {
                        if (cantVidas < 1)
                        {
                            AdministradorDeEscenarios.getSingleton().agregarEscenario(new GameOver(), camaraInterna);
                        }
                        personajePrincipal.Position = puntoCheckpointActual;
                        cantVidas--;
                    }
                }
            }
        }

        private void verSiSeCompletoNivel(TgcMesh mesh)
        {

            if (mesh.Name.Contains("Box_8"))
            {
                if (cantidadLibrosAdquiridos >= 10)
                {
                    reproductorMp3.closeFile();
                    AdministradorDeEscenarios.getSingleton().agregarEscenario(new Intermedio(), camaraInterna);
                } else
                {
                    personajePrincipal.Position = new TGCVector3(652, 13, 9815);
                }
              
            }

        }

        private void AgarrarLibros(TgcMesh mesh)
        {
            if (mesh.Name == "Box_1" && !librosAgarrados.Contains(mesh))
            {
                librosAgarrados.Add(mesh);
                cantidadLibrosAdquiridos++;
                mesh.BoundingBox = new Core.BoundingVolumes.TgcBoundingAxisAlignBox();
                mesh.Dispose();
            }
        }

        private void Slider(TGCVector3 lastPos, TGCVector3 movementRay, float dtime)
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

            rs.Scale(0.2f*sliderModifier);
            handleSliderModifier();

            if (!enElPiso && !techo)
                rs.Y = -jump * dtime;
            else if (techo)
            {
                rs.Y = Math.Abs(personajePrincipal.BoundingBox.PMax.Y - ceilingCollider.BoundingBox.PMax.Y);
                techo = false;
            }
            personajePrincipal.Position = lastPos - rs;
        }
        private void ajustarPosicionDeCamara()
        {
            //Actualizar valores de camara segun modifiers
            camaraInterna.OffsetHeight = 150;
            camaraInterna.OffsetForward = 300;
            var displacement = new TGCVector3(0,60,200);
            camaraInterna.TargetDisplacement = new TGCVector3(displacement.X, displacement.Y, 0);

            //Pedirle a la camara cual va a ser su proxima posicion
            TGCVector3 position;
            TGCVector3 target;
            camaraInterna.CalculatePositionTarget(out position, out target);

            //Detectar colisiones entre el segmento de recta camara-personaje y todos los objetos del escenario
            TGCVector3 q;
            var minDistSq = FastMath.Pow2(camaraInterna.OffsetForward);
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
                    if (Math.Abs(mesh.BoundingBox.PMax.Y - mesh.BoundingBox.PMin.Y) < 60 )
                        continue;
                    if (TgcCollisionUtils.intersectSegmentAABB(target, position, mesh.BoundingBox, out q))
                    {
                        //Si hay colision, guardar la que tenga menor distancia
                        float distSq = TGCVector3.Subtract(q, target).LengthSq();
                        //Hay dos casos singulares, puede que tengamos mas de una colision hay que quedarse con el menor offset.
                        //Si no dividimos la distancia por 2 se acerca mucho al target.
                        minDistSq = FastMath.Min(distSq * 0.75f, minDistSq);
                    }
                }
            }
            float newOffsetForward = -FastMath.Sqrt(minDistSq);

            if (FastMath.Abs(newOffsetForward) < 10)
            {
                newOffsetForward = 10;
            }
            camaraInterna.OffsetForward = -newOffsetForward;
            camaraInterna.CalculatePositionTarget(out position, out target);
            camaraInterna.SetCamera(position, target);
        }

        private void handleSliderModifier()
        {
            if (sliderModifierType == "slow")
                handleSlowSliderModifier();
            else if (sliderModifierType == "fast")
                handleFastSliderModifier();
        }

        private void handleFastSliderModifier()
        {
            if (floorCollider == null || sliderFloorCollider == null || (floorCollider != sliderFloorCollider && enElPiso))
                sliderModifier = 1;
        }

        private void handleSlowSliderModifier()
        {
            if (floorCollider == null || sliderFloorCollider == null || floorCollider != sliderFloorCollider)
                sliderModifier = 1;
        }
        private TGCVector3 getClosestLight(TGCVector3 pos)
        {
            var minDist = float.MaxValue;
            TgcMesh minLight = null;

            foreach (var light in lights)
            {
                var distSq = TGCVector3.LengthSq(pos - light.BoundingBox.calculateBoxCenter());
                if (distSq < minDist)
                {
                    minDist = distSq;
                    minLight = light;
                }
            }
            return minLight.BoundingBox.calculateBoxCenter();
        }

        private void animarBolasDeCanionYEmisoresDeParticulas(float deltaTime)
        {
            incrementoBola1 = velocidadDesplazamientoBolasDeCanion * deltaTime * (-1);
            incrementoBola2 = velocidadDesplazamientoBolasDeCanion * deltaTime * (-1.5f);
            incrementoBola3 = velocidadDesplazamientoBolasDeCanion * deltaTime * (-2);

            bolaDeCanion1.Move(0, 0, incrementoBola1);
            emisorDeParticulas1.Position = new TGCVector3(emisorDeParticulas1.Position.X, emisorDeParticulas1.Position.Y, emisorDeParticulas1.Position.Z + incrementoBola1);
            distanciaRecorridaBola1 = distanciaRecorridaBola1 + incrementoBola1;
            if (Math.Abs(distanciaRecorridaBola1) > 3000f)
            {
                bolaDeCanion1.Position = posicionInicialBolaDeCanion1;
                emisorDeParticulas1.Position = posicionInicialEmisorDeParticulas1;
                distanciaRecorridaBola1 = 0f;
            }

            bolaDeCanion2.Move(0, 0, incrementoBola2);
            emisorDeParticulas2.Position = new TGCVector3(emisorDeParticulas2.Position.X, emisorDeParticulas2.Position.Y, emisorDeParticulas2.Position.Z + incrementoBola2);
            distanciaRecorridaBola2 = distanciaRecorridaBola2 + incrementoBola2;
            if (Math.Abs(distanciaRecorridaBola2) > 3000f)
            {
                bolaDeCanion2.Position = posicionInicialBolaDeCanion2;
                emisorDeParticulas2.Position = posicionInicialEmisorDeParticulas2;
                distanciaRecorridaBola2 = 0f;
            }

            bolaDeCanion3.Move(0, 0, incrementoBola3);
            emisorDeParticulas3.Position = new TGCVector3(emisorDeParticulas3.Position.X, emisorDeParticulas3.Position.Y, emisorDeParticulas3.Position.Z + incrementoBola3);
            distanciaRecorridaBola3 = distanciaRecorridaBola3 + incrementoBola3;
            if (Math.Abs(distanciaRecorridaBola3) > 3000f)
            {
                bolaDeCanion3.Position = posicionInicialBolaDeCanion3;
                emisorDeParticulas3.Position = posicionInicialEmisorDeParticulas3;
                distanciaRecorridaBola3 = 0f;
            }
        }

        private void animarLibrosF1(float deltaTime)
        {
            foreach (TgcMesh libro in scene.Meshes)
            {
                if (libro.Name == "Box_1" && !librosAgarrados.Contains(libro))
                {
                    incremento = velocidadDesplazamientolibros * direccionDeMovimientoActualLibrosF1 * deltaTime;
                    libro.Move(0, incremento, 0);
                    distanciaRecorrida = distanciaRecorrida + incremento;
                    if (Math.Abs(distanciaRecorrida) > 1000f)
                    {
                        direccionDeMovimientoActualLibrosF1 *= -1;
                        distanciaRecorrida = 0f;
                    }
                }
            }
        }

        private void animarPlataformas(float deltaTime)
        {
            plataforma1.Move(0, velocidadDesplazamientoPlataformas * direccionDeMovimientoActualPlataforma1 * deltaTime, 0);
            if (FastMath.Abs(plataforma1.Position.Y) > 300f)
            {
                direccionDeMovimientoActualPlataforma1 *= -1;
            }

            plataforma2.Move(0, velocidadDesplazamientoPlataformas * (-direccionDeMovimientoActualPlataforma2) * deltaTime, 0);
            if (FastMath.Abs(plataforma2.Position.Y) > 300f)
            {
                direccionDeMovimientoActualPlataforma2 *= -1;
            }
        }
        private void AplicarShaders(String shaderDir)
        {
            efectoOlas = TgcShaders.loadEffect(shaderDir + "ShaderOlas.fx");

            foreach (TgcMesh mesh in aguas)
            {
                mesh.Effect = efectoOlas;
                mesh.Technique = "Olas";
            }
            efectoOlas.SetValue("screen_dx", resolucionX);
            efectoOlas.SetValue("screen_dy", resolucionY);
        }

        private void cambiarSigno()
        {
            signo *= -1;
        }

        //11, 21, 26, 30, 34, 42, 57
        private void SetearAguas()
        {
            aguas.Add(scene.Meshes[11]);
            aguas.Add(scene.Meshes[21]);
            aguas.Add(scene.Meshes[26]);
            aguas.Add(scene.Meshes[30]);
            aguas.Add(scene.Meshes[34]);
            aguas.Add(scene.Meshes[42]);
            aguas.Add(scene.Meshes[57]);
        }
    }
}