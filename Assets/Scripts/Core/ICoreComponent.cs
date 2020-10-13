using System.Collections.Generic;

public interface ICoreComponent {

    void Initialize (IEnumerable<ICoreComponent> allCoreComponents);

}
