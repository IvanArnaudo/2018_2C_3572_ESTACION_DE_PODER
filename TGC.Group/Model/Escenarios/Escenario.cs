using TGC.Core.Camara;
using TGC.Core.Input;
using TGC.Core.Example;

namespace TGC.Group.Model.Escenarios
{
    interface Escenario{
        void render(float deltaTime);
        void update(float deltaTime, TgcD3dInput input, TgcCamera camara);
        void dispose();
        void init(string mediaDir, string shaderDir);
    }
}
