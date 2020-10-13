using System.Collections.Generic;

public interface ICoreComponent {

    void InitializeCoreComponent (IEnumerable<ICoreComponent> allCoreComponents);

}
