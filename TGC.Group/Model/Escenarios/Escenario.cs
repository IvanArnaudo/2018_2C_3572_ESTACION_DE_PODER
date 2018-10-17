using TGC.Core.Camara;
using TGC.Core.Input;
using TGC.Core.Example;

namespace TGC.Group.Model.Escenarios
{
    interface Escenario{
        void init(string mediaDir, string shaderDir, TgcCamera camara);
        void update(float deltaTime, TgcD3dInput input, TgcCamera camara);
        void render(float deltaTime);
        void dispose();
    }
}
