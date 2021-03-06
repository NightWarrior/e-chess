﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardStructure {

	public enum UnitTeam {
		WHITE,
		BLACK,
		NULL}

	;

	public struct tileStruct {
		public UnitTeam team;
		public UnitType piece;
	}

	public tileStruct[,] board; 
	Vector2 whiteKing, blackKing;

	public BoardStructure () {
		board = new tileStruct[8, 8];

		whiteKing = new Vector2 ();
		blackKing = new Vector2 ();


		// initialize all the pseudo-tiles by what is at the original board at the given time
		for (int i = 0; i < 8; i++) {
			for (int j = 0; j < 8; j++) {

				board [i, j].piece = GameObject.Find ("White").GetComponent<White> ().getUnitTypeAtPosition (new Vector3 (i, 0, j)); // first check if a white piece at the place
				if (board [i, j].piece != UnitType.NULL) {
					board [i, j].team = UnitTeam.WHITE;
					if (board [i, j].piece == UnitType.KING)
						whiteKing = new Vector2 (i, j);
				} else { // if no white then it initializes to NULL
					// check if a black is at this location, else NUll is initialized
					board [i, j].piece = GameObject.Find ("Black").GetComponent<Black> ().getUnitTypeAtPosition (new Vector3 (i, 0, j)); 
					if (board [i, j].piece != UnitType.NULL) {
						board [i, j].team = UnitTeam.BLACK;
						if (board [i, j].piece == UnitType.KING)
							blackKing = new Vector2 (i, j);
					} else
						board [i, j].team = UnitTeam.NULL;
				}
			}
		}
	}

	public BoardStructure (tileStruct [,] b) {
		board = new tileStruct[8, 8];

		whiteKing = new Vector2 ();
		blackKing = new Vector2 ();


		// initialize all the pseudo-tiles by what is at the original board at the given time
		for (int i = 0; i < 8; i++) {
			for (int j = 0; j < 8; j++) {

				board [i, j].team = b [i, j].team;
				board [i, j].piece = b [i, j].piece;

				if (board [i, j].piece == UnitType.KING && board[i,j].team == UnitTeam.WHITE)
					whiteKing = new Vector2 (i, j);
				if (board [i, j].piece == UnitType.KING && board[i,j].team == UnitTeam.BLACK)
					blackKing = new Vector2 (i, j);
			}
		}
	}

	public int makeCall(tileStruct[,] b, Vector3 from, Vector3 to){
		BoardStructure newB = new BoardStructure (b);
		if(!newB.checkMovable (from, to))
			return -200;
		int val = 0;

		UnitTeam team = newB.board [(int)from.x, (int)from.z].team; // team of the unit to be moved
		UnitType targetUnit = newB.board [(int)to.x, (int)to.z].piece; // to check if there is an enemy unit at the other end


		newB.move (from, to);
		if (team == UnitTeam.BLACK) {// checking if opponent is in check
			if (newB.isWhiteCheck ()) {
				val += 200;
			}
		} else
			if(newB.isBlackCheck ()) {
			val += 200;
		}

		if (targetUnit == UnitType.QUEEN)
			val += 100;
		else if (targetUnit == UnitType.PAWN)
			val += 20;
		else if (targetUnit == UnitType.ROOK || targetUnit == UnitType.BISHOP || targetUnit == UnitType.KNIGHT)
			val += 50;
		


		return val;
	}

	public tileStruct[,] getBoardAfterMove(tileStruct[,] b, Vector3 from, Vector3 to){
		BoardStructure newB = new BoardStructure (b);
		newB.move (from, to);
		return newB.board; // Note: Might Cause error if its held in reffence and unallocated when needed.

	}


	public int alphabeta(int nodeVal, int depth, int alpha, int beta, bool player, tileStruct[,] b){ // player, true for white, false for black
		if(depth == 0)
			return nodeVal;
		int bestVal;
		int highlights = 0; // if highlights are still 0 at the end, means enemy has done checkmate, reurn a -1000
		if (player) {
			bestVal = int.MinValue;
			for (int i = 0; i < 8; i++) {
				for (int j = 0; j < 8; j++) {
					UnitType unitType = board [i, j].piece;
					if (board [i, j].team == UnitTeam.WHITE) {
						if (board [i, j].piece == UnitType.PAWN) {
							if (j == 1) { // 1 for white
								// Check if pawn is at initial stage and can move two steps
								if (board [i, j + 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;


									if (board [i, j + 2].piece == UnitType.NULL) {
										highlights++;
										newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 2));
										if (newVal != -200)
											newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 2)));
										bestVal = Mathf.Max (newVal, bestVal);
										alpha = Mathf.Max (alpha, bestVal);
										if (beta <= alpha)
											return bestVal;
									}
								}
							} else {
								// Check rest of the times
								if (board [i, j + 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}
							}

							if (board [i + 1, j + 1].team == UnitTeam.BLACK && board [i + 1, j + 1].piece != UnitType.KING) {
								if (board [i + 1, j + 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;

								}
							}
							if (board [i - 1, j + 1].team == UnitTeam.BLACK && board [i - 1, j + 1].piece != UnitType.KING) {
								if (board [i - 1, j + 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}
							}
						} else if (board [i, j].piece == UnitType.KING) { ////////////////////////////////////////////////////////////////////////////////
							if (board [i, j + 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i + 1, j + 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i + 1, j].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i + 1, j - 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j - 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j - 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i, j - 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j - 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j - 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i - 1, j - 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j - 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j - 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i - 1, j].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i - 1, j + 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}
							//king attacks: 

						
							if (board [i, j + 1].team == UnitTeam.BLACK && board [i, j + 1].piece != UnitType.KING) {
								if (board [i, j + 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j + 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i + 1, j + 1].team == UnitTeam.BLACK && board [i + 1, j + 1].piece != UnitType.KING) {
								if (board [i + 1, j + 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i + 1, j].team == UnitTeam.BLACK && board [i + 1, j].piece != UnitType.KING) {
								if (board [i + 1, j].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i + 1, j - 1].team == UnitTeam.BLACK && board [i + 1, j - 1].piece != UnitType.KING) {
								if (board [i + 1, j - 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j - 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j - 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i, j - 1].team == UnitTeam.BLACK && board [i, j - 1].piece != UnitType.KING) {
								if (board [i, j - 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j - 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i, 0, j - 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i - 1, j - 1].team == UnitTeam.BLACK && board [i - 1, j - 1].piece != UnitType.KING) {
								if (board [i - 1, j - 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j - 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j - 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i - 1, j].team == UnitTeam.BLACK && board [i - 1, j].piece != UnitType.KING) {
								if (board [i - 1, j].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i - 1, j + 1].team == UnitTeam.BLACK && board [i - 1, j + 1].piece != UnitType.KING) {
								if (board [i - 1, j + 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}
							}
						
						} else if (board [i, j].piece == UnitType.KNIGHT) {////////////////////////////////////////////////////////////////////////////////
							if (board [i + 1, j + 2].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 2));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 2)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i + 2, j + 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 2, 0, j + 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 2, 0, j + 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i + 1, j - 2].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j - 2));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j - 2)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i + 2, j - 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 2, 0, j - 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 2, 0, j - 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i - 1, j - 2].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j - 2));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j - 2)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i - 2, j - 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 2, 0, j - 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 2, 0, j - 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i - 1, j + 2].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 2));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 2)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}

							if (board [i - 2, j + 1].piece == UnitType.NULL) {
								highlights++;
								int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 2, 0, j + 1));
								if (newVal != -200)
									newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 2, 0, j + 1)));
								bestVal = Mathf.Max (newVal, bestVal);
								alpha = Mathf.Max (alpha, bestVal);
								if (beta <= alpha)
									return bestVal;
							}
							//Knight attacks: 


							if (board [i, j + 1].team == UnitTeam.BLACK && board [i, j + 1].piece != UnitType.KING) {
								if (board [i + 1, j + 2].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 2));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j + 2)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i + 2, j + 1].team == UnitTeam.BLACK && board [i + 2, j + 1].piece != UnitType.KING) {
								if (board [i + 2, j + 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 2, 0, j + 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 2, 0, j + 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i + 1, j - 2].team == UnitTeam.BLACK && board [i + 1, j - 2].piece != UnitType.KING) {
								if (board [i + 1, j - 2].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j - 2));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 1, 0, j - 2)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i + 2, j - 1].team == UnitTeam.BLACK && board [i + 2, j - 1].piece != UnitType.KING) {
								if (board [i + 2, j - 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + 2, 0, j - 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i + 2, 0, j - 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i - 1, j - 2].team == UnitTeam.BLACK && board [i - 1, j - 2].piece != UnitType.KING) {
								if (board [i - 1, j - 2].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j - 2));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j - 2)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i - 2, j - 1].team == UnitTeam.BLACK && board [i - 2, j - 1].piece != UnitType.KING) {
								if (board [i - 2, j - 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 2, 0, j - 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 2, 0, j - 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i - 1, j + 2].team == UnitTeam.BLACK && board [i - 1, j + 2].piece != UnitType.KING) {
								if (board [i - 1, j + 2].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 2));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 1, 0, j + 2)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

							}
							if (board [i - 2, j + 1].team == UnitTeam.BLACK && board [i - 2, j + 1].piece != UnitType.KING) {
								if (board [i - 2, j + 1].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i - 2, 0, j + 1));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i - 2, 0, j + 1)));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}
							}
						}
						else if (board [i, j].piece == UnitType.ROOK) {////////////////////////////////////////////////////////////////////////////////
							bool breaker = false;
							for (int k = 0; k < 8; k++) {
								if (breaker == true)
									break;

								if (board [i + k, j ].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + k, 0, j ));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i  + k, 0, j )));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

								if (board [i + k, j ].team == UnitTeam.BLACK && board [i + k, j ].piece != UnitType.KING) {
									if (board [i + k, j ].piece == UnitType.NULL) {
										highlights++;
										int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + k, 0, j ));
										if (newVal != -200)
											newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i  + k, 0, j )));
										bestVal = Mathf.Max (newVal, bestVal);
										alpha = Mathf.Max (alpha, bestVal);
										if (beta <= alpha)
											return bestVal;
									}
									breaker = true;
									break;
								}
							}


							breaker = false; ////// change the directional values //////////////////////////////////////////////////////////////////////////////////////////
							for (int k = 0; k < 8; k++) {
								if (breaker == true)
									break;

								if (board [i + k, j ].piece == UnitType.NULL) {
									highlights++;
									int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + k, 0, j ));
									if (newVal != -200)
										newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i  + k, 0, j )));
									bestVal = Mathf.Max (newVal, bestVal);
									alpha = Mathf.Max (alpha, bestVal);
									if (beta <= alpha)
										return bestVal;
								}

								if (board [i + k, j ].team == UnitTeam.BLACK && board [i + k, j ].piece != UnitType.KING) {
									if (board [i + k, j ].piece == UnitType.NULL) {
										highlights++;
										int newVal = makeCall (b, new Vector3 (i, 0, j), new Vector3 (i + k, 0, j ));
										if (newVal != -200)
											newVal = alphabeta (nodeVal, depth - 1, alpha, beta, false, getBoardAfterMove (b, new Vector3 (i, 0, j), new Vector3 (i  + k, 0, j )));
										bestVal = Mathf.Max (newVal, bestVal);
										alpha = Mathf.Max (alpha, bestVal);
										if (beta <= alpha)
											return bestVal;
									}
									breaker = true;
									break;
								}
							}


						}
						else if (board [i, j].piece == UnitType.BISHOP) {////////////////////////////////////////////////////////////////////////////////

						}
						else if (board [i, j].piece == UnitType.QUEEN) {////////////////////////////////////////////////////////////////////////////////
						}

						
					}
				}
			}
			if (highlights == 0)
				return -1000;
			return bestVal;
		} else {
			bestVal = int.MaxValue;
			for (int i = 0; i < 8; i++) {
				for (int j = 0; j < 8; j++) {

				}
			}
			if (highlights == 0)
				return -1000;
			return bestVal;
		}
	}


	public bool checkMovable (Vector3 from, Vector3 to) { // checking if move is valid or own team goes into check from making the move
		bool moveable = true;
		UnitTeam team = board [(int)from.x, (int)from.z].team; // team of the unit to be moved

		//updateBoard ();
		move (from, to);
		if (team == UnitTeam.BLACK) {// team of unit we are checking for movement is checked
			moveable = isBlackCheck ();
		} else
			moveable = isWhiteCheck ();
		move (to, from);

		return !moveable;

		//		return false;
	}

	public void move (Vector3 from, Vector3 to) {
		if (board [(int)from.x, (int)from.z].piece == UnitType.KING) {
			if (board [(int)from.x, (int)from.z].team == UnitTeam.BLACK) { // updating the separate record of each king
				blackKing.x = to.x;
				blackKing.y = to.z;
			} else {
				whiteKing.x = to.x;
				whiteKing.y = to.z;
			}
		}

		board [(int)to.x, (int)to.z].piece = board [(int)from.x, (int)from.z].piece;
		board [(int)to.x, (int)to.z].team = board [(int)from.x, (int)from.z].team;
		board [(int)from.x, (int)from.z].piece = UnitType.NULL;
		board [(int)from.x, (int)from.z].team = UnitTeam.NULL;

	}


	public void updateBoard () {

		// initialize all the pseudo-tiles by what is at the original board at the given time
		for (int i = 0; i < 8; i++) {
			for (int j = 0; j < 8; j++) {
				board [i, j].team = UnitTeam.NULL;
				board [i, j].piece = UnitType.NULL;

				board [i, j].piece = GameObject.Find ("White").GetComponent<White> ().getUnitTypeAtPosition (new Vector3 (i, 0, j)); // first check if a white piece at the place
				if (board [i, j].piece != UnitType.NULL) {
					board [i, j].team = UnitTeam.WHITE;
					if (board [i, j].piece == UnitType.KING)
						whiteKing = new Vector2 (i, j);
				} else { // if no white then it initializes to NULL
					// check if a black is at this location, else NUll is initialized
					board [i, j].piece = GameObject.Find ("Black").GetComponent<Black> ().getUnitTypeAtPosition (new Vector3 (i, 0, j)); 
					if (board [i, j].piece != UnitType.NULL) {
						board [i, j].team = UnitTeam.BLACK;
						if (board [i, j].piece == UnitType.KING)
							blackKing = new Vector2 (i, j);
					} else
						board [i, j].team = UnitTeam.NULL;
				}
			}
		}
	}

	public bool isWhiteCheck () { // checking if white is in check with the current situation
		bool[] lineChecks = new bool[4]; // used to keep track of each x or + paths pieces to see how far they extend 

		// check king's
		if ((int)whiteKing.x + 1 < 8)
		if (board [(int)whiteKing.x + 1, (int)whiteKing.y].team == UnitTeam.BLACK && board [(int)whiteKing.x + 1, (int)whiteKing.y].piece == UnitType.KING)
			return true;
		if ((int)whiteKing.y + 1 < 8)
		if (board [(int)whiteKing.x, (int)whiteKing.y + 1].team == UnitTeam.BLACK && board [(int)whiteKing.x, (int)whiteKing.y + 1].piece == UnitType.KING)
			return true;
		if ((int)whiteKing.x - 1 > 0)
		if (board [(int)whiteKing.x - 1, (int)whiteKing.y].team == UnitTeam.BLACK && board [(int)whiteKing.x - 1, (int)whiteKing.y].piece == UnitType.KING)
			return true;
		if ((int)whiteKing.y - 1 > 0)
		if (board [(int)whiteKing.x, (int)whiteKing.y - 1].team == UnitTeam.BLACK && board [(int)whiteKing.x, (int)whiteKing.y - 1].piece == UnitType.KING)
			return true;
		if ((int)whiteKing.x + 1 < 8 && (int)whiteKing.y + 1 < 8)
		if (board [(int)whiteKing.x + 1, (int)whiteKing.y + 1].team == UnitTeam.BLACK && board [(int)whiteKing.x + 1, (int)whiteKing.y + 1].piece == UnitType.KING)
			return true;
		if ((int)whiteKing.x + 1 < 8 && (int)whiteKing.y - 1 > 0)
		if (board [(int)whiteKing.x + 1, (int)whiteKing.y - 1].team == UnitTeam.BLACK && board [(int)whiteKing.x + 1, (int)whiteKing.y - 1].piece == UnitType.KING)
			return true;
		if ((int)whiteKing.x - 1 > 0 && (int)whiteKing.y + 1 < 8)
		if (board [(int)whiteKing.x - 1, (int)whiteKing.y + 1].team == UnitTeam.BLACK && board [(int)whiteKing.x - 1, (int)whiteKing.y + 1].piece == UnitType.KING)
			return true;
		if ((int)whiteKing.x - 1 > 0 && (int)whiteKing.y - 1 > 0)
		if (board [(int)whiteKing.x - 1, (int)whiteKing.y - 1].team == UnitTeam.BLACK && board [(int)whiteKing.x - 1, (int)whiteKing.y - 1].piece == UnitType.KING)
			return true;

		// check knight's 
		if ((int)whiteKing.x + 1 < 8 && (int)whiteKing.y + 2 < 8)
		if (board [(int)whiteKing.x + 1, (int)whiteKing.y + 2].team == UnitTeam.BLACK && board [(int)whiteKing.x + 1, (int)whiteKing.y + 2].piece == UnitType.KNIGHT)
			return true;
		if ((int)whiteKing.x + 1 < 8 && (int)whiteKing.y - 2 > 0)
		if (board [(int)whiteKing.x + 1, (int)whiteKing.y - 2].team == UnitTeam.BLACK && board [(int)whiteKing.x + 1, (int)whiteKing.y - 2].piece == UnitType.KNIGHT)
			return true;
		if ((int)whiteKing.x + 2 < 8 && (int)whiteKing.y + 1 < 8)
		if (board [(int)whiteKing.x + 2, (int)whiteKing.y + 1].team == UnitTeam.BLACK && board [(int)whiteKing.x + 2, (int)whiteKing.y + 1].piece == UnitType.KNIGHT)
			return true;
		if ((int)whiteKing.x + 2 < 8 && (int)whiteKing.y - 1 > 0)
		if (board [(int)whiteKing.x + 2, (int)whiteKing.y - 1].team == UnitTeam.BLACK && board [(int)whiteKing.x + 2, (int)whiteKing.y - 1].piece == UnitType.KNIGHT)
			return true;
		if ((int)whiteKing.x - 1 > 0 && (int)whiteKing.y + 2 < 8)
		if (board [(int)whiteKing.x - 1, (int)whiteKing.y + 2].team == UnitTeam.BLACK && board [(int)whiteKing.x - 1, (int)whiteKing.y + 2].piece == UnitType.KNIGHT)
			return true;
		if ((int)whiteKing.x - 1 > 0 && (int)whiteKing.y - 2 > 0)
		if (board [(int)whiteKing.x - 1, (int)whiteKing.y - 2].team == UnitTeam.BLACK && board [(int)whiteKing.x - 1, (int)whiteKing.y - 2].piece == UnitType.KNIGHT)
			return true;
		if ((int)whiteKing.x - 2 > 0 && (int)whiteKing.y + 1 < 8)
		if (board [(int)whiteKing.x - 2, (int)whiteKing.y + 1].team == UnitTeam.BLACK && board [(int)whiteKing.x - 2, (int)whiteKing.y + 1].piece == UnitType.KNIGHT)
			return true;
		if ((int)whiteKing.x - 2 > 0 && (int)whiteKing.y - 1 > 0)
		if (board [(int)whiteKing.x - 2, (int)whiteKing.y - 1].team == UnitTeam.BLACK && board [(int)whiteKing.x - 2, (int)whiteKing.y - 1].piece == UnitType.KNIGHT)
			return true;

		// check bishop, queen, pawn
		lineChecks [0] = lineChecks [1] = lineChecks [2] = lineChecks [3] = true;
		for (int i = 1; i < 8; i++) {
			if (lineChecks [0] && (int)whiteKing.x + i < 8 && (int)whiteKing.y + i < 8) {
				if (board [(int)whiteKing.x + i, (int)whiteKing.y + i].team == UnitTeam.BLACK && board [(int)whiteKing.x + i, (int)whiteKing.y + i].piece == UnitType.BISHOP)
					return true;
				if (board [(int)whiteKing.x + i, (int)whiteKing.y + i].team == UnitTeam.BLACK && board [(int)whiteKing.x + i, (int)whiteKing.y + i].piece == UnitType.QUEEN)
					return true;
				if (i == 1 && board [(int)whiteKing.x + i, (int)whiteKing.y + i].team == UnitTeam.BLACK && board [(int)whiteKing.x + i, (int)whiteKing.y + i].piece == UnitType.PAWN)
					return true;

				if (board [(int)whiteKing.x + i, (int)whiteKing.y + i].team != UnitTeam.NULL) // if the path isnt clear then its skip the rest in that line
					lineChecks [0] = false;
			}
			if (lineChecks [1] && (int)whiteKing.x + i < 8 && (int)whiteKing.y - i >= 0) {
				if (board [(int)whiteKing.x + i, (int)whiteKing.y - i].team == UnitTeam.BLACK && board [(int)whiteKing.x + i, (int)whiteKing.y - i].piece == UnitType.BISHOP)
					return true;
				if (board [(int)whiteKing.x + i, (int)whiteKing.y - i].team == UnitTeam.BLACK && board [(int)whiteKing.x + i, (int)whiteKing.y - i].piece == UnitType.QUEEN)
					return true;

				if (board [(int)whiteKing.x + i, (int)whiteKing.y - i].team != UnitTeam.NULL)
					lineChecks [1] = false;
			}
			if (lineChecks [2] && (int)whiteKing.x - i >= 0 && (int)whiteKing.y + i < 8) {
				if (board [(int)whiteKing.x - i, (int)whiteKing.y + i].team == UnitTeam.BLACK && board [(int)whiteKing.x - i, (int)whiteKing.y + i].piece == UnitType.BISHOP)
					return true;
				if (board [(int)whiteKing.x - i, (int)whiteKing.y + i].team == UnitTeam.BLACK && board [(int)whiteKing.x - i, (int)whiteKing.y + i].piece == UnitType.QUEEN)
					return true;
				if (i == 1 && board [(int)whiteKing.x - i, (int)whiteKing.y + i].team == UnitTeam.BLACK && board [(int)whiteKing.x - i, (int)whiteKing.y + i].piece == UnitType.PAWN)
					return true;

				if (board [(int)whiteKing.x - i, (int)whiteKing.y + i].team != UnitTeam.NULL)
					lineChecks [2] = false;
			}
			if (lineChecks [3] && (int)whiteKing.x - i >= 0 && (int)whiteKing.y - i >= 0) {
				if (board [(int)whiteKing.x - i, (int)whiteKing.y - i].team == UnitTeam.BLACK && board [(int)whiteKing.x - i, (int)whiteKing.y - i].piece == UnitType.BISHOP)
					return true;
				if (board [(int)whiteKing.x - i, (int)whiteKing.y - i].team == UnitTeam.BLACK && board [(int)whiteKing.x - i, (int)whiteKing.y - i].piece == UnitType.QUEEN)
					return true;

				if (board [(int)whiteKing.x - i, (int)whiteKing.y - i].team != UnitTeam.NULL)
					lineChecks [3] = false;
			}

		}

		// check rook, queen
		lineChecks [0] = lineChecks [1] = lineChecks [2] = lineChecks [3] = true;
		for (int i = 1; i < 8; i++) {
			if ((int)whiteKing.x + i < 8) {
				if (lineChecks [0]) {
					if (board [(int)whiteKing.x + i, (int)whiteKing.y].team == UnitTeam.BLACK && board [(int)whiteKing.x + i, (int)whiteKing.y].piece == UnitType.ROOK)
						return true;
					if (board [(int)whiteKing.x + i, (int)whiteKing.y].team == UnitTeam.BLACK && board [(int)whiteKing.x + i, (int)whiteKing.y].piece == UnitType.QUEEN)
						return true;

					if (board [(int)whiteKing.x + i, (int)whiteKing.y].team != UnitTeam.NULL)
						lineChecks [0] = false;
				}
			}
			if ((int)whiteKing.y + i < 8) {
				if (lineChecks [1]) {
					if (board [(int)whiteKing.x, (int)whiteKing.y + i].team == UnitTeam.BLACK && board [(int)whiteKing.x, (int)whiteKing.y + i].piece == UnitType.ROOK)
						return true;
					if (board [(int)whiteKing.x, (int)whiteKing.y + i].team == UnitTeam.BLACK && board [(int)whiteKing.x, (int)whiteKing.y + i].piece == UnitType.QUEEN)
						return true;

					if (board [(int)whiteKing.x, (int)whiteKing.y + i].team != UnitTeam.NULL)
						lineChecks [1] = false;
				}
			}
			if ((int)whiteKing.x - i >= 0) {
				if (lineChecks [2]) {
					if (board [(int)whiteKing.x - i, (int)whiteKing.y].team == UnitTeam.BLACK && board [(int)whiteKing.x - i, (int)whiteKing.y].piece == UnitType.ROOK)
						return true;
					if (board [(int)whiteKing.x - i, (int)whiteKing.y].team == UnitTeam.BLACK && board [(int)whiteKing.x - i, (int)whiteKing.y].piece == UnitType.QUEEN)
						return true;

					if (board [(int)whiteKing.x - i, (int)whiteKing.y].team != UnitTeam.NULL)
						lineChecks [2] = false;
				}

			}
			if ((int)whiteKing.y - i >= 0) {
				if (lineChecks [3]) {
					if (board [(int)whiteKing.x, (int)whiteKing.y - i].team == UnitTeam.BLACK && board [(int)whiteKing.x, (int)whiteKing.y - i].piece == UnitType.ROOK)
						return true;
					if (board [(int)whiteKing.x, (int)whiteKing.y - i].team == UnitTeam.BLACK && board [(int)whiteKing.x, (int)whiteKing.y - i].piece == UnitType.QUEEN)
						return true;

					if (board [(int)whiteKing.x, (int)whiteKing.y - i].team != UnitTeam.NULL)
						lineChecks [3] = false;
				}
			}
		}

		return false;
	}

	public bool isBlackCheck () {
		bool[] lineChecks = new bool[4];

		// check king's
		if ((int)blackKing.x + 1 < 8)
		if (board [(int)blackKing.x + 1, (int)blackKing.y].team == UnitTeam.WHITE && board [(int)blackKing.x + 1, (int)blackKing.y].piece == UnitType.KING)
			return true;
		if ((int)blackKing.y + 1 < 8)
		if (board [(int)blackKing.x, (int)blackKing.y + 1].team == UnitTeam.WHITE && board [(int)blackKing.x, (int)blackKing.y + 1].piece == UnitType.KING)
			return true;
		if ((int)blackKing.x - 1 > 0)
		if (board [(int)blackKing.x - 1, (int)blackKing.y].team == UnitTeam.WHITE && board [(int)blackKing.x - 1, (int)blackKing.y].piece == UnitType.KING)
			return true;
		if ((int)blackKing.y - 1 > 0)
		if (board [(int)blackKing.x, (int)blackKing.y - 1].team == UnitTeam.WHITE && board [(int)blackKing.x, (int)blackKing.y - 1].piece == UnitType.KING)
			return true;
		if ((int)blackKing.x + 1 < 8 && (int)blackKing.y + 1 < 8)
		if (board [(int)blackKing.x + 1, (int)blackKing.y + 1].team == UnitTeam.WHITE && board [(int)blackKing.x + 1, (int)blackKing.y + 1].piece == UnitType.KING)
			return true;
		if ((int)blackKing.x + 1 < 8 && (int)blackKing.y - 1 > 0)
		if (board [(int)blackKing.x + 1, (int)blackKing.y - 1].team == UnitTeam.WHITE && board [(int)blackKing.x + 1, (int)blackKing.y - 1].piece == UnitType.KING)
			return true;
		if ((int)blackKing.x - 1 > 0 && (int)blackKing.y + 1 < 8)
		if (board [(int)blackKing.x - 1, (int)blackKing.y + 1].team == UnitTeam.WHITE && board [(int)blackKing.x - 1, (int)blackKing.y + 1].piece == UnitType.KING)
			return true;
		if ((int)blackKing.x - 1 > 0 && (int)blackKing.y - 1 > 0)
		if (board [(int)blackKing.x - 1, (int)blackKing.y - 1].team == UnitTeam.WHITE && board [(int)blackKing.x - 1, (int)blackKing.y - 1].piece == UnitType.KING)
			return true;


		// check knight's 
		if ((int)blackKing.x + 1 < 8 && (int)blackKing.y + 2 < 8)
		if (board [(int)blackKing.x + 1, (int)blackKing.y + 2].team == UnitTeam.WHITE && board [(int)blackKing.x + 1, (int)blackKing.y + 2].piece == UnitType.KNIGHT)
			return true;
		if ((int)blackKing.x + 1 < 8 && (int)blackKing.y - 2 > 0)
		if (board [(int)blackKing.x + 1, (int)blackKing.y - 2].team == UnitTeam.WHITE && board [(int)blackKing.x + 1, (int)blackKing.y - 2].piece == UnitType.KNIGHT)
			return true;
		if ((int)blackKing.x + 2 < 8 && (int)blackKing.y + 1 < 8)
		if (board [(int)blackKing.x + 2, (int)blackKing.y + 1].team == UnitTeam.WHITE && board [(int)blackKing.x + 2, (int)blackKing.y + 1].piece == UnitType.KNIGHT)
			return true;
		if ((int)blackKing.x + 2 < 8 && (int)blackKing.y - 1 > 0)
		if (board [(int)blackKing.x + 2, (int)blackKing.y - 1].team == UnitTeam.WHITE && board [(int)blackKing.x + 2, (int)blackKing.y - 1].piece == UnitType.KNIGHT)
			return true;
		if ((int)blackKing.x - 1 > 0 && (int)blackKing.y + 2 < 8)
		if (board [(int)blackKing.x - 1, (int)blackKing.y + 2].team == UnitTeam.WHITE && board [(int)blackKing.x - 1, (int)blackKing.y + 2].piece == UnitType.KNIGHT)
			return true;
		if ((int)blackKing.x - 1 > 0 && (int)blackKing.y - 2 > 0)
		if (board [(int)blackKing.x - 1, (int)blackKing.y - 2].team == UnitTeam.WHITE && board [(int)blackKing.x - 1, (int)blackKing.y - 2].piece == UnitType.KNIGHT)
			return true;
		if ((int)blackKing.x - 2 > 0 && (int)blackKing.y + 1 < 8)
		if (board [(int)blackKing.x - 2, (int)blackKing.y + 1].team == UnitTeam.WHITE && board [(int)blackKing.x - 2, (int)blackKing.y + 1].piece == UnitType.KNIGHT)
			return true;
		if ((int)blackKing.x - 2 > 0 && (int)blackKing.y - 1 > 0)
		if (board [(int)blackKing.x - 2, (int)blackKing.y - 1].team == UnitTeam.WHITE && board [(int)blackKing.x - 2, (int)blackKing.y - 1].piece == UnitType.KNIGHT)
			return true;

		// check bishop, queen, pawn
		lineChecks [0] = lineChecks [1] = lineChecks [2] = lineChecks [3] = true;
		for (int i = 1; i < 8; i++) {
			if (lineChecks [0] && (int)blackKing.x + i < 8 && (int)blackKing.y + i < 8) {
				if (board [(int)blackKing.x + i, (int)blackKing.y + i].team == UnitTeam.WHITE && board [(int)blackKing.x + i, (int)blackKing.y + i].piece == UnitType.BISHOP)
					return true;
				if (board [(int)blackKing.x + i, (int)blackKing.y + i].team == UnitTeam.WHITE && board [(int)blackKing.x + i, (int)blackKing.y + i].piece == UnitType.QUEEN)
					return true;

				if (board [(int)blackKing.x + i, (int)blackKing.y + i].team != UnitTeam.NULL) // if the path isnt clear then its skip the rest in that line
					lineChecks [0] = false;
			}
			if (lineChecks [1] && (int)blackKing.x + i < 8 && (int)blackKing.y - i >= 0) {
				if (board [(int)blackKing.x + i, (int)blackKing.y - i].team == UnitTeam.WHITE && board [(int)blackKing.x + i, (int)blackKing.y - i].piece == UnitType.BISHOP)
					return true;
				if (board [(int)blackKing.x + i, (int)blackKing.y - i].team == UnitTeam.WHITE && board [(int)blackKing.x + i, (int)blackKing.y - i].piece == UnitType.QUEEN)
					return true;
				if (i == 1 && board [(int)blackKing.x + i, (int)blackKing.y - i].team == UnitTeam.WHITE && board [(int)blackKing.x + i, (int)blackKing.y - i].piece == UnitType.PAWN)
					return true;

				if (board [(int)blackKing.x + i, (int)blackKing.y - i].team != UnitTeam.NULL)
					lineChecks [1] = false;
			}
			if (lineChecks [2] && (int)blackKing.x - i >= 0 && (int)blackKing.y + i < 8) {
				if (board [(int)blackKing.x - i, (int)blackKing.y + i].team == UnitTeam.WHITE && board [(int)blackKing.x - i, (int)blackKing.y + i].piece == UnitType.BISHOP)
					return true;
				if (board [(int)blackKing.x - i, (int)blackKing.y + i].team == UnitTeam.WHITE && board [(int)blackKing.x - i, (int)blackKing.y + i].piece == UnitType.QUEEN)
					return true;

				if (board [(int)blackKing.x - i, (int)blackKing.y + i].team != UnitTeam.NULL)
					lineChecks [2] = false;
			}
			if (lineChecks [3] && (int)blackKing.x - i >= 0 && (int)blackKing.y - i >= 0) {
				if (board [(int)blackKing.x - i, (int)blackKing.y - i].team == UnitTeam.WHITE && board [(int)blackKing.x - i, (int)blackKing.y - i].piece == UnitType.BISHOP)
					return true;
				if (board [(int)blackKing.x - i, (int)blackKing.y - i].team == UnitTeam.WHITE && board [(int)blackKing.x - i, (int)blackKing.y - i].piece == UnitType.QUEEN)
					return true;
				if (i == 1 && board [(int)blackKing.x - i, (int)blackKing.y - i].team == UnitTeam.WHITE && board [(int)blackKing.x - i, (int)blackKing.y - i].piece == UnitType.PAWN)
					return true;

				if (board [(int)blackKing.x - i, (int)blackKing.y - i].team != UnitTeam.NULL)
					lineChecks [3] = false;
			}

		}

		// check rook, queen
		lineChecks [0] = lineChecks [1] = lineChecks [2] = lineChecks [3] = true;
		for (int i = 1; i < 8; i++) {
			if ((int)blackKing.x + i < 8) {
				if (lineChecks [0]) {
					if (board [(int)blackKing.x + i, (int)blackKing.y].team == UnitTeam.WHITE && board [(int)blackKing.x + i, (int)blackKing.y].piece == UnitType.ROOK)
						return true;
					if (board [(int)blackKing.x + i, (int)blackKing.y].team == UnitTeam.WHITE && board [(int)blackKing.x + i, (int)blackKing.y].piece == UnitType.QUEEN)
						return true;

					if (board [(int)blackKing.x + i, (int)blackKing.y].team != UnitTeam.NULL)
						lineChecks [0] = false;
				}
			}
			if ((int)blackKing.y + i < 8) {
				if (lineChecks [1]) {
					//Debug.Log (blackKing + " " + (int)blackKing.y + i);
					if (board [(int)blackKing.x, (int)blackKing.y + i].team == UnitTeam.WHITE && board [(int)blackKing.x, (int)blackKing.y + i].piece == UnitType.ROOK)
						return true;
					if (board [(int)blackKing.x, (int)blackKing.y + i].team == UnitTeam.WHITE && board [(int)blackKing.x, (int)blackKing.y + i].piece == UnitType.QUEEN)
						return true;

					if (board [(int)blackKing.x, (int)blackKing.y + i].team != UnitTeam.NULL)
						lineChecks [1] = false;
				}
			}
			if ((int)blackKing.x - i >= 0) {
				if (lineChecks [2]) {
					if (board [(int)blackKing.x - i, (int)blackKing.y].team == UnitTeam.WHITE && board [(int)blackKing.x - i, (int)blackKing.y].piece == UnitType.ROOK)
						return true;
					if (board [(int)blackKing.x - i, (int)blackKing.y].team == UnitTeam.WHITE && board [(int)blackKing.x - i, (int)blackKing.y].piece == UnitType.QUEEN)
						return true;

					if (board [(int)blackKing.x - i, (int)blackKing.y].team != UnitTeam.NULL)
						lineChecks [2] = false;
				}
			}
			if ((int)blackKing.y - i >= 0) {
				if (lineChecks [3]) {
					if (board [(int)blackKing.x, (int)blackKing.y - i].team == UnitTeam.WHITE && board [(int)blackKing.x, (int)blackKing.y - i].piece == UnitType.ROOK)
						return true;
					if (board [(int)blackKing.x, (int)blackKing.y - i].team == UnitTeam.WHITE && board [(int)blackKing.x, (int)blackKing.y - i].piece == UnitType.QUEEN)
						return true;

					if (board [(int)blackKing.x, (int)blackKing.y - i].team != UnitTeam.NULL)
						lineChecks [3] = false;
				}
			}
		}

		return false;
	}


//	public bool isWhiteCheckMate(){
//
//	}

}
