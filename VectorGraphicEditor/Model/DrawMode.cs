namespace VectorGraphicEditor.Model
{
    public enum DrawMode
    {
        /// <summary>
        /// Не определен
        /// </summary>
        None,
        /// <summary>
        /// Добавление фигуры
        /// </summary>
        AddNewFigure,
        /// <summary>
        /// Редактирование фигуры
        /// </summary>
        EditFigure,
        /// <summary>
        /// Редактирование вершин
        /// </summary>
        MoveVertex,
        /// <summary>
        /// Двигать фигуру
        /// </summary>
        MoveFigure
    }
}
