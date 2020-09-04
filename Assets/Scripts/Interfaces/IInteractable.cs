public interface IInteractable {

    bool CanBeInteractedWith { get; }       // as in, completely disabled. interactions that don't do anything should be handled in the actual script.

    string InteractionDescription { get; }

    void Interact (object interactor);
	
}
