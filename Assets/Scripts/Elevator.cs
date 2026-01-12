using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class Elevator : MonoBehaviour
{
    [Header("Cabin")]
    [SerializeField] private Transform elevatorCabin;
    [SerializeField] private float moveUpDistance = 6f;
    [SerializeField] private float moveDuration = 2f;

    [Header("Doors - Current Floor")]
    [SerializeField] private Transform currentLeftDoor;
    [SerializeField] private Transform currentRightDoor;

    [Header("Doors - Next Floor")]
    [SerializeField] private Transform nextLeftDoor;
    [SerializeField] private Transform nextRightDoor;

    [Header("Door Settings")]
    [SerializeField] private float doorMoveDistance = 1.2f;
    [SerializeField] private float doorMoveDuration = 0.6f;

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("cat")) return;

        used = true;
        GetComponent<Collider>().enabled = false;

        StartElevatorSequence();
    }

    private void StartElevatorSequence()
    {
        AudioManager.instance.play("elevator start");
        Sequence seq = DOTween.Sequence();
        Ease doorEase = Ease.InOutCubic;
        Ease moveEase = Ease.InOutSine;

        // -----------------------------
        // Close current floor doors
        // -----------------------------
        seq.Append(
            currentLeftDoor
                .DOLocalMoveX(currentLeftDoor.localPosition.x - doorMoveDistance, doorMoveDuration)
                .SetEase(doorEase)
        );

        seq.Join(
            currentRightDoor
                .DOLocalMoveX(currentRightDoor.localPosition.x + doorMoveDistance, doorMoveDuration)
                .SetEase(doorEase)
        );
        // -----------------------------
        // Move elevator cabin UP
        // -----------------------------
        seq.Append(
            elevatorCabin
                .DOLocalMoveY(elevatorCabin.localPosition.y + moveUpDistance, moveDuration)
                .SetEase(moveEase)
        );
        AudioManager.instance.play("elevator reached");
        // -----------------------------
        // Open next floor doors
        // -----------------------------
        seq.Append(
            nextLeftDoor
                .DOLocalMoveX(nextLeftDoor.localPosition.x + doorMoveDistance, doorMoveDuration)
                .SetEase(doorEase)
        );

        seq.Join(
            nextRightDoor
                .DOLocalMoveX(nextRightDoor.localPosition.x - doorMoveDistance, doorMoveDuration)
                .SetEase(doorEase)
        );
    }
}
