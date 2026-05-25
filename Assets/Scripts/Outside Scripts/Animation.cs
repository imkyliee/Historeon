using UnityEngine;

public class Animation : MonoBehaviour
{
    public Movement movement;
    private Animator animator; 
    public string currentAnimation = "";

    private void ChangeAnimation(string animation, float crossfade = 0.2f)
    {
        if (currentAnimation != animation)
        {
            currentAnimation = animation;
            animator.CrossFade(animation, crossfade);  
        }
    }
    private void CheckedAnimation()
    {
        if(movement.move.y == 1)
        {
            ChangeAnimation("Run");
        }
        else if(movement.move.y == 0)
        {
            ChangeAnimation("Idle");
        }
    }
    void Start()
    {
        CheckedAnimation();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
