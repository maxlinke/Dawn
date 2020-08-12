public interface IInteractable {

    bool CanBeInteractedWith { get; }

    string InteractionDescription { get; }

    void Interact (object interactor);
	
}
