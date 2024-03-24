using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamba2 : MonoBehaviour
{
    public LambaTipi lambaTipi;
    public int xIndex;
    public int yIndex;

    public bool isMatched;

    private Vector2 currentPos;
    private Vector2 targetPos;

    public bool isMoving;


    public void SetIndicies(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    //hedefe hareket
    public void MoveToTarget(Vector2 _targetPos)
    {
        StartCoroutine(MoveCoroutine(_targetPos));
    }
    //rutine hareket ettir
    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isMoving = true;
        float duration = 0.2f;

        Vector2 startPosition = transform.position;
        float elaspedTime = 0f;

        while (elaspedTime < duration)
        {
            float t = elaspedTime / duration;

            transform.position = Vector2.Lerp(startPosition, _targetPos, t);

            elaspedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _targetPos;
        isMoving = false;

    }

}