using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
	public static BoardManager instance;
	public GameObject tile;
	private int xSize, ySize;

	private GameObject[,] tiles;

	public bool IsShifting { get; set; }

	public GameObject ExplosionEF;

	[Header("Data")] 
	[SerializeField] private MapSO mapSO;
	
	void Start()
	{
		instance = GetComponent<BoardManager>();

		SetData();
		Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
		CreateBoard(offset.x, offset.y);
	}

	void SetData()
	{
		xSize = mapSO.Levels[0].xSize;
		ySize = mapSO.Levels[0].ysize;
	}

	public bool IsDarkCell(int xPos, int yPos)
	{
		var data = mapSO.Levels[0].listDarkCell;
		var darkCell = new MapSO.Level.DarkCell();
		darkCell.xPos = xPos;
		darkCell.yPos = yPos;
		foreach (var darkCellMember in data)
		{
			if(darkCell.xPos == darkCellMember.xPos
			   && darkCell.yPos == darkCellMember.yPos)
				return true;
		}

		return false;
	}

	private void CreateBoard(float xOffset, float yOffset)
	{
		tiles = new GameObject[xSize, ySize];
		float startX = transform.position.x;
		float startY = transform.position.y;

		Sprite[] previousLeft = new Sprite[ySize];
		Sprite previousBelow = null;

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				var isDarkCell = IsDarkCell(x, y);
				GetTile(startX, startY, x, y, xOffset, yOffset, previousLeft, previousBelow, isDarkCell);
			}
		}
	}

	public void GetTile(float startX, float startY, int x, int y, float xOffset,
		float yOffset, Sprite[] previousLeft, Sprite previousBelow
		, bool isDarkCell)
	{
		GameObject newTile = Instantiate(tile,
			new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
		tiles[x, y] = newTile;

		newTile.transform.parent = transform;

		List<Sprite> possibleCharacters = new List<Sprite>();
		possibleCharacters.AddRange(GetTileSprites(isDarkCell));

		if (!isDarkCell)
		{
			possibleCharacters.Remove(previousLeft[y]);
			possibleCharacters.Remove(previousBelow);
		}
		else
		{
			newTile.GetComponent<BoxCollider2D>().enabled = false;
		}
		
		//Sprite newSprite = characters[Random.Range(0, characters.Count)];

		Sprite newSprite;
		if(isDarkCell)
			newSprite = possibleCharacters[0];
		else
			newSprite = possibleCharacters[Random.Range(0, possibleCharacters.Count)];

		newTile.GetComponent<SpriteRenderer>().sprite = newSprite;

		previousLeft[y] = newSprite;
		previousBelow = newSprite;
	}

	public List<Sprite> GetTileSprites(bool isDarkCell)
	{
		var characters = mapSO.Levels[0].characters;
		if (!isDarkCell)
			return characters;

		var darkCells = mapSO.Levels[0].darkCells;
		return darkCells;
	}

	public IEnumerator FindNullTiles()
	{
		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				var isDarkCell = IsDarkCell(x, y);
				if (/*!isDarkCell &&*/ tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
				{
					yield return StartCoroutine(ShiftTilesDown(x, y));
					break;
				}
			}
		}

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				var isDarkCell = IsDarkCell(x, y);
				if(!isDarkCell)
					tiles[x, y].GetComponent<Tile>().ClearAllMatches();
			}
		}

	}

	private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f)
	{
		IsShifting = true;
		List<SpriteRenderer> renders = new List<SpriteRenderer>();
		int nullCount = 0;

		for (int y = yStart; y < ySize; y++)
		{ 
			var isDarkCell = IsDarkCell(x, y);
			if (!isDarkCell)
			{
				SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
				if (render.sprite == null)
				{
					nullCount++;
				}
				renders.Add(render);
			}
		}

		for (int i = 0; i < nullCount; i++)
		{ 
			yield return new WaitForSeconds(shiftDelay);			

			for (int k = 0; k < renders.Count - 1; k++)
			{
				renders[k].sprite = renders[k + 1].sprite;
				//renders[k + 1].sprite = null;
				/*var isDarkCell = IsDarkCell(x, ySize - 1);
				if (!isDarkCell)*/
				renders[k + 1].sprite = GetNewSprite(x, ySize - 1);
			}
		}
		IsShifting = false;
	}

	private Sprite GetNewSprite(int x, int y)
	{
		List<Sprite> possibleCharacters = new List<Sprite>();
		var characters = mapSO.Levels[0].characters;
		possibleCharacters.AddRange(characters);
		
		if (x > 0/* && !IsDarkCell(x - 1, y)*/)
		{
			possibleCharacters.Remove(tiles[x - 1, y].GetComponent<SpriteRenderer>().sprite);
		}

		if (x < xSize - 1 /*&& !IsDarkCell(x + 1, y)*/)
		{
			possibleCharacters.Remove(tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite);
		}

		if (y > 0 /*&& !IsDarkCell(x, y - 1)*/)
		{
			possibleCharacters.Remove(tiles[x, y - 1].GetComponent<SpriteRenderer>().sprite);
		}

		return possibleCharacters[Random.Range(0, possibleCharacters.Count)];
	}

}
