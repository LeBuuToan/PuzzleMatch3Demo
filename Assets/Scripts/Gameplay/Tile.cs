using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine.Events;

public class Tile : MonoBehaviour
{
	private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
	private static Tile previousSelected = null;
	private static Tile _previousMatch = null;
	
	private SpriteRenderer _render;
	private bool isSelected = false;

	private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

	private bool _matchFound;
	private bool _isSwapAnim = true;
	private Vector3 _block1Pos;
	private Vector3 _block2Pos;

	public static event Action<bool> matchFoundEvent;
		
	void Awake() {
		_render = GetComponent<SpriteRenderer>();
		//matchFoundEvent += i => ShiftingTiles();
	}

	private void Select() {
		isSelected = true;
		_render.color = selectedColor;
		previousSelected = gameObject.GetComponent<Tile>();
		SFXManager.instance.PlaySFX(Clip.Select);
	}

	private void Deselect() {
		isSelected = false;
		_render.color = Color.white;
		previousSelected = null;
	}

	async void OnMouseDown()
	{
		if (GameManager.instance.paused)
			return;
		
		if (_render.sprite == null || BoardManager.instance.IsShifting)
		{
			return;
		}

		if (isSelected)
		{
			Deselect();
		}
		else
		{
			if (previousSelected == null)
			{
				Select();
			}
			else
			{
				if (GetAllAdjacentTiles().Contains(previousSelected.gameObject)
				&& _render.sprite != previousSelected._render.sprite)
				{
					_previousMatch = previousSelected;
					var obj1 = _render.GetComponent<Transform>();
					var obj2 = previousSelected._render.GetComponent<Transform>();
					await SwapAnimation(obj1, obj2);
					_block1Pos = obj1.transform.position;
					_block2Pos = obj2.transform.position;
					SwapSprite(previousSelected._render, true);
					previousSelected.ClearAllMatches();
					previousSelected.Deselect();
					ClearAllMatches();
					MatchingFail();
				}
				else
				{ // 3
					previousSelected.GetComponent<Tile>().Deselect();
					Select();
				}
			}

		}
	}

	public async void SwapSprite(SpriteRenderer render2, bool isMouseDown)
	{
		if (_render.sprite == render2.sprite)
		{
			return;
		}
		

		var tempSprite = render2.sprite;
		render2.sprite = _render.sprite;
		_render.sprite = tempSprite;

		if (isMouseDown)
		{
			SFXManager.instance.PlaySFX(Clip.Swap);
			GUIManager.instance.MoveCounter--;
		}
	}
	
	public async Task<Task> SwapAnimation(Transform obj1, Transform obj2)
	{
		var tempObj = obj2;
		obj2.transform.DOMove(obj1.position, 0.5f);
		obj1.transform.DOMove(tempObj.position, 0.5f);
		return Task.CompletedTask;
	}

	public async Task<Task> SwapOldPosition(Vector3 block1Pos, Vector3 block2Pos)
	{
		transform.DOMove(block1Pos, 0.5f);
		var obj2 = _previousMatch.GetComponent<Transform>();
		obj2.transform.DOMove(block2Pos, 0.5f);
		return Task.CompletedTask;
	}

	public async void MatchingFail()
	{
		if (!_isSwapAnim)
		{
			await SwapOldPosition(_block1Pos, _block2Pos);
			
			SwapSprite(_previousMatch._render, false);
			_isSwapAnim = true;
		}
	}

	private GameObject GetAdjacent(Vector2 castDir)
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
		if (hit.collider != null)
		{
			return hit.collider.gameObject;
		}
		return null;
	}

	private List<GameObject> GetAllAdjacentTiles()
	{
		List<GameObject> adjacentTiles = new List<GameObject>();
		for (int i = 0; i < adjacentDirections.Length; i++)
		{
			adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
		}
		return adjacentTiles;
	}

	private List<GameObject> FindMatch(Vector2 castDir)
	{ 
		List<GameObject> matchingTiles = new List<GameObject>(); 
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir); 
		while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == _render.sprite)
		{ 
			matchingTiles.Add(hit.collider.gameObject);
			hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
		}
		return matchingTiles; 
	}

	private async Task<Task>  ClearMatch(Vector2[] paths) 
	{
		List<GameObject> matchingTiles = new List<GameObject>(); 
		for (int i = 0; i < paths.Length; i++) 
		{
			matchingTiles.AddRange(FindMatch(paths[i]));
		}

		if (matchingTiles.Count >= 2) 
		{
			for (int i = 0; i < matchingTiles.Count; i++) 
			{
				matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;			
			}
			_matchFound = true;
			
			//OnMatchFoundEvent(matchingTiles.Count);
			GUIManager.instance.Score += 20	 * matchingTiles.Count;
			
			Instantiate(BoardManager.instance.ExplosionEF, _render.transform.position, Quaternion.identity);
		}
		else
		{
			_isSwapAnim = false;
		}
		
		return Task.CompletedTask;
	}


	public void ShiftingTiles()
	{
		if (_matchFound)
		{
			_render.sprite = null;
			_matchFound = false;

			StopCoroutine(BoardManager.instance.FindNullTiles());
			StartCoroutine(BoardManager.instance.FindNullTiles());

			SFXManager.instance.PlaySFX(Clip.Clear);

			GUIManager.instance.ExplosionCounter--;
		}

	}
	
	public async void ClearAllMatches()
	{
		if (_render.sprite == null)
			return;

		//ClearMatch(new Vector2[4] { Vector2.up, Vector2.down, Vector2.left, Vector2.right });
		await ClearMatch(new Vector2[2] { Vector2.left, Vector2.right });
		await ClearMatch(new Vector2[2] { Vector2.up, Vector2.down });
		

		if (_matchFound)
		{
			_render.sprite = null;
			_matchFound = false;

			StopCoroutine(BoardManager.instance.FindNullTiles());
			StartCoroutine(BoardManager.instance.FindNullTiles());

			SFXManager.instance.PlaySFX(Clip.Clear);

			GUIManager.instance.ExplosionCounter--;		
		}
	}

	private static void OnMatchFoundEvent(bool isMatch)
	{
		matchFoundEvent?.Invoke(isMatch);
	}
}