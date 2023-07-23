using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;
using KModkit;

public class UnicodeArrowsScr : MonoBehaviour {
    public KMSelectable[] ArrowSelectables;
    public KMSelectable Undo;
    public SpriteRenderer[] ArrowRenderers;
    public Sprite[] ArrowSprites;

    public KMBombModule module;
    public KMAudio sound;

    int[][] SpriteArray = new int[][]{
    new int[]{0,1,2,3},
    new int[]{4,5,6,7},
    new int[]{8,9,10,11},
    new int[]{12,13,14,15} };
    int[][] DirectionArray;
    int[] DirectionTable = new int[] {3, 2, 1, 4, -1, 0, 5, 6, 7 };

    List<int> history = new List<int>();
    bool UndoButtonHeld;
    int[][] initialSpriteArray;
    int[][] initialDirectionArray;

    bool solved;
    int loggingId;
    static int loggingIdCounter;

    List<int> autosolverHistory = new List<int>();
    bool scrambling;
    // Use this for initialization
    private void Start()
    {

        loggingId = loggingIdCounter++;
        for (int i = 0; i < 16; i++)
        {
            int arrow = i;
            ArrowSelectables[i].OnInteract += PressArrow(arrow);
        }
        Undo.OnInteract += HandleUndoPress();
        int[] no4 = new int[] { 0, 1, 2, 3, 5, 6, 7, 8 };
        DirectionArray = new int[][]{
new int[]{no4[rnd.Range(0, 8)], no4[rnd.Range(0, 8)], no4[rnd.Range(0, 8)],  no4[rnd.Range(0, 8)]},
new int[]{no4[rnd.Range(0, 8)], no4[rnd.Range(0, 8)], no4[rnd.Range(0, 8)],  no4[rnd.Range(0, 8)]},
new int[]{no4[rnd.Range(0, 8)], no4[rnd.Range(0, 8)], no4[rnd.Range(0, 8)],  no4[rnd.Range(0, 8)]},
new int[]{no4[rnd.Range(0, 8)], no4[rnd.Range(0, 8)], no4[rnd.Range(0, 8)],  no4[rnd.Range(0, 8)]} };
        Debug.LogFormat("[Unicode Arrows #{0}] Arrow rotations: {1}", loggingId, DirectionArray.Select(x => x.Select(y => 45 * DirectionTable[y]).Join(",")).Join("\n"));
        int ScramNum = rnd.Range(7, 12);
        for (int i = 0; i < ScramNum; i++)
        {
            Debug.LogFormat("[Unicode Arrows #{0}] Scramble:", loggingId);
            scrambling = true;
            Swap(rnd.Range(0, 4), rnd.Range(0, 4), -1);
            scrambling = false;
        }
        Debug.LogFormat("[Unicode Arrows #{0}] Input:", loggingId);
        UpdateArrows();
    }

	void Swap (int arrowX, int arrowY, int backwards) {
    int destinationX = arrowX;
    int destinationY = arrowY;
		if (DirectionArray[arrowY][arrowX] % 3 == 0)
        {
            destinationX = arrowX - backwards;
            if (destinationX == 4)
                destinationX = 0;
            if (destinationX == -1)
                destinationX = 3;
        }
        if (DirectionArray[arrowY][arrowX] % 3 == 2)
        {
            destinationX = arrowX + backwards;
            if (destinationX == 4)
                destinationX = 0;
            if (destinationX == -1)
                destinationX = 3;
        }
    
    if (DirectionArray[arrowY][arrowX] / 3 == 0)
        {
            destinationY = arrowY - backwards;
            if (destinationY == 4)
                destinationY = 0;
            if (destinationY == -1)
                destinationY = 3;
        }
        if (DirectionArray[arrowY][arrowX] / 3 == 2)
        {
            destinationY = arrowY + backwards;
            if (destinationY == 4)
                destinationY = 0;
            if (destinationY == -1)
                destinationY = 3;
        }
        int temp = SpriteArray[arrowY][arrowX];
        SpriteArray[arrowY][arrowX] = SpriteArray[destinationY][destinationX];
        SpriteArray[destinationY][destinationX] = temp;
        temp = DirectionArray[arrowY][arrowX];
        DirectionArray[arrowY][arrowX] = DirectionArray[destinationY][destinationX];
        DirectionArray[destinationY][destinationX] = temp;
        Debug.LogFormat("[Unicode Arrows #{0}] Board: {1}", loggingId, SpriteArray.Select(x => x.Join(",")).Join("\n"));
        if (backwards == 1)
            history.Add(4 * destinationY + destinationX);
        if (scrambling)
            autosolverHistory.Add(4 * destinationY + destinationX);
    }
    void UpdateArrows()
    {
        for (int i = 0; i < 16; i++)
        {
            ArrowRenderers[i].sprite = ArrowSprites[SpriteArray[i / 4][i % 4]];
            ArrowRenderers[i].transform.localEulerAngles = new Vector3(0, 0, 45 * DirectionTable[DirectionArray[i / 4][i % 4]]);
       }
    }
    void CheckSolve()
    {
        for (int i = 0; i < 16; i++)
        {
            if (SpriteArray[i / 4][i % 4] != i)
            {
                return;
            }
        }
        sound.PlaySoundAtTransform("Solve", transform);
        solved = true;
        module.HandlePass();
    }
    KMSelectable.OnInteractHandler PressArrow(int arrow)
    {
        return delegate
        {
            ArrowSelectables[arrow].AddInteractionPunch(0.5f);
            sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if (!solved)
            {
                Swap(arrow % 4, arrow / 4, 1);
                UpdateArrows();
                CheckSolve();
            }
            return false;
        };
    }
    KMSelectable.OnInteractHandler HandleUndoPress()
    {
        return delegate
        {
            if (!solved)
            {
                Debug.LogFormat("[Unicode Arrows #{0}] Undoing last move.", loggingId);
                Undo.AddInteractionPunch(0.5f);
                sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                if (history.Count > 0)
                {
                    Swap(history[history.Count - 1] % 4, history[history.Count - 1] / 4, -1);
                    history.RemoveAt(history.Count - 1);
                }
                UpdateArrows();
                CheckSolve();
            }
            return false;
        };
    }
    string TwitchHelpMessage = "Use e.g. '!{0} 1,2,3,4' to press the arrows in those positions in reading order. Use '!{0} undo n' to undo N moves. Use '!{0} reset' to reset the module.";
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        if (command == "reset")
        {
            yield return null;
            while (history.Count > 0)
            {
                Undo.OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
        }
        else if (command.StartsWith("undo "))
        {
            string[] cmdArray = command.Split(' ');
            int UndoNum;
            if (cmdArray.Length < 2)
            {
                yield return "sendtochaterror Too many paramaters!";
                yield break;
            }
            if (cmdArray.Length > 2)
            {
                yield return "sendtochaterror Specify a number of moves to undo!";
                yield break;
            }
            if (!int.TryParse(cmdArray[1], out UndoNum))
            {
                yield return string.Format("sendtochaterror Parameter '{0}' not a number!", cmdArray[1]);
                yield break;
            }
            int step = 0;
            yield return null;
            while (step < UndoNum)
            {
                Undo.OnInteract();
                yield return new WaitForSeconds(0.2f);
                step++;
            }       
        }
        else
        {
            string[] cmdArray = command.Split(',');
            List<int> userArrows = new List<int>();
            int curArrow;
            foreach (string i in cmdArray)
            {
                if (!int.TryParse(i, out curArrow))
                {
                    yield return string.Format("sendtochaterror Parameter '{0}' not a number!", i);
                    yield break;
                }
                if (curArrow > 16 || curArrow < 1)
                {
                    yield return string.Format("sendtochaterror Parameter '{0}' out of range!", i);
                    yield break;
                }
                userArrows.Add(curArrow);
            }
            yield return null;
            foreach (int i in userArrows)
            {
                ArrowSelectables[i - 1].OnInteract();
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        while (history.Count > 0)
        {
            Undo.OnInteract();
            yield return new WaitForSeconds(0.2f);
        }
        while (autosolverHistory.Count > 0)
        {
             ArrowSelectables[autosolverHistory[autosolverHistory.Count - 1]].OnInteract();
             autosolverHistory.RemoveAt(autosolverHistory.Count - 1);
             yield return new WaitForSeconds(0.2f);
        }      
    }
}
