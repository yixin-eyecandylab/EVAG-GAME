using System;

public class HighScore : IComparable {
//	public int id;

    public readonly string player;
    public readonly string email;
	public readonly int score;

	public int CompareTo(object obj) {
		if(obj == null) return 1;
		HighScore otherScore = obj as HighScore;
		if(otherScore != null)
			return this.score.CompareTo(otherScore.score);
		else
			throw new ArgumentException("Object is not a HighScore");
	}

	public HighScore(string newPlayer, string newEmail, int newScore) {
        if (string.IsNullOrEmpty(newPlayer))
        {
            player = "unknown";
        } else
        {
            player = newPlayer;
        }
        if (string.IsNullOrEmpty(newEmail))
        {
            email = "unknown";
        }
        else
        {
            email = newEmail;
        }
       
		score = newScore;
	}

	public HighScore() {
		player = null;
		score = -999;
	}

}

