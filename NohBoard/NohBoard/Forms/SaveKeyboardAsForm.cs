/*
Copyright (C) 2016 by Eric Bataille <e.c.p.bataille@gmail.com>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace ThoNohT.NohBoard.Forms
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Extra;

    /// <summary>
    /// The form used to save a keyboard under a new name.
    /// </summary>
    public partial class SaveKeyboardAsForm : Form
    {
        /// <summary>
        /// The name of the currently selected category.
        /// </summary>
        private string SelectedCategory => this.CategoryCombo.Text.SanitizeFilename();

        /// <summary>
        /// The name of the currently selected defintion.
        /// </summary>
        private string SelectedDefinition => this.DefinitionCombo.Text.SanitizeFilename();

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveKeyboardAsForm" /> class.
        /// </summary>
        public SaveKeyboardAsForm()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes the form, pre-filling the list of categories and definitions in the current category.
        /// </summary>
        private void SaveKeyboardAsForm_Load(object sender, EventArgs e)
        {
            this.ApplyLocalizedSaveKeyboardTexts();

            var root = FileHelper.FromKbs();

            // If there are no keyboard files, no initialization is required.
            if (!root.Exists) return;

            this.CategoryCombo.Items.AddRange(root.EnumerateDirectories().Select(x => (object)x.Name).ToArray());

            this.CategoryCombo.Text = GlobalSettings.Settings.LoadedCategory;
            this.DefinitionCombo.Text = GlobalSettings.Settings.LoadedKeyboard;
        }

        private void ApplyLocalizedSaveKeyboardTexts()
        {
            var L = UiTranslate.Lang;

            this.Text = UiTranslate.T(L, "Save Keyboard Definition", "儲存鍵盤定義", "保存键盘定义", "キーボード定義を保存");
            this.lblCategory.Text = UiTranslate.T(L, "Category:", "分類：", "类别：", "カテゴリ：");
            this.lblName.Text = UiTranslate.T(L, "Name:", "名稱：", "名称：", "名前：");
            this.SaveButton.Text = UiTranslate.T(L, "Save", "儲存", "保存", "保存");
            this.CancelButton2.Text = UiTranslate.T(L, "Cancel", "取消", "取消", "キャンセル");
        }

        /// <summary>
        /// Populates the list of keyboards for the specified category.
        /// </summary>
        /// <param name="category">The category to load the keyboards from.</param>
        private void PopulateKeyboards(string category)
        {
            var root = FileHelper.FromKbs(category);
            if (!root.Exists) return;

            this.DefinitionCombo.Items.Clear();
            this.DefinitionCombo.Items.AddRange(root.EnumerateDirectories().Select(x => (object)x.Name).ToArray());
        }

        /// <summary>
        /// Handles the change of the category combo selection change, re-populates keyboards.
        /// </summary>
        private void CategoryCombo_TextChanged(object sender, EventArgs e)
        {
            this.PopulateKeyboards(this.SelectedCategory);
        }

        /// <summary>
        /// Saves the keyboard definition in the specified location.
        /// </summary>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.SelectedCategory) || string.IsNullOrWhiteSpace(this.SelectedDefinition))
                return;

            // Check for a reserved name.
            if (string.Equals(
                this.SelectedCategory,
                Constants.GlobalStylesFolder,
                StringComparison.InvariantCultureIgnoreCase))
            {
                var result = MessageBox.Show(
                    this,
                    string.Format(
                        UiTranslate.T(
                            "{0} cannot be used for a category name.",
                            "分類名稱不能使用「{0}」。",
                            "类别名称不能使用「{0}」。",
                            "カテゴリ名に「{0}」は使えません。"),
                        Constants.GlobalStylesFolder),
                    UiTranslate.T("Invalid name", "名稱無效", "名称无效", "無効な名前"),
                    MessageBoxButtons.OKCancel);

                if (result == DialogResult.OK)
                    return;

                this.Close();
                return;
            }

            // Check if the name already exists.
            if (FileHelper.FromKbs(this.SelectedCategory, this.SelectedDefinition).Exists)
            {
                var result = MessageBox.Show(
                    this,
                    string.Format(
                        UiTranslate.T(
                            "Keyboard {0}/{1} already exists, do you want to overwrite it?",
                            "鍵盤 {0}/{1} 已存在，要覆寫嗎？",
                            "键盘 {0}/{1} 已存在，要覆盖吗？",
                            "キーボード {0}/{1} は既にあります。上書きしますか？"),
                        this.SelectedCategory,
                        this.SelectedDefinition),
                    UiTranslate.T("Already exists", "已存在", "已存在", "既に存在します"),
                    MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.No) return;
                if (result == DialogResult.Cancel)
                {
                    this.Close();
                    return;
                }
            }

            // Save the definition.
            GlobalSettings.CurrentDefinition.Category = this.SelectedCategory;
            GlobalSettings.CurrentDefinition.Name = this.SelectedDefinition;
            GlobalSettings.CurrentDefinition.Save();
            GlobalSettings.Settings.LoadedCategory = this.SelectedCategory;
            GlobalSettings.Settings.LoadedKeyboard = this.SelectedDefinition;
            this.Close();
        }
    }
}
