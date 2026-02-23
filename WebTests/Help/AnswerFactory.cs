namespace WebTests.TestFactory
{
    public static class AnswerFactory
    {
        public static double CalculateScore(List<int> selectedIds, List<int> correctIds)
        {
            if (!correctIds.Any())
                return 0;

            int correctSelected = selectedIds.Intersect(correctIds).Count();
            int wrongSelected = selectedIds.Except(correctIds).Count();

            double score = (double)correctSelected / selectedIds.Count;

            //if (wrongSelected > 0)
            //    score -= (double)correctSelected / correctIds.Count;

            return Math.Max(0, Math.Min(1, score));
        }
    }
}
