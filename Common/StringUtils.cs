using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public static class StringUtils
    {

        /// <summary>
        /// AddReticence - Adiciona reticências num texto quando necessário.
        /// </summary>
        /// <param name="text">Texto que ser quer adicionar reticências</param>
        /// <param name="qtdMinToApply">Quantidade de caractres minha que o texto deve ter para serem aplicadas as reticências.</param>
        /// <param name="txtToAddInEnd">Texto opcinal para ser adicionado após as reticências.</param>
        /// <returns>Retorna o texto com reticências aplicada quando necessário, ou o prorio texto recebido no parametro se não necessário.</returns>
        public static string AddReticence(string text, int qtdMinToApply, string txtToAddInEnd = "") {
            const string reticences = "...";
            if (string.IsNullOrWhiteSpace(text)) {
                return text;
            }
            if (text.Length < qtdMinToApply) {
                return text;
            }

            ///Se o texto resultante ficar de tamanho maior ou igual ao do texto original, então retorna o texto original;
            if (qtdMinToApply + reticences.Length + (txtToAddInEnd?.Length ?? 0) >= text.Length) {
                return text;
            }
            return $"{text.AsSpan(0, qtdMinToApply).TrimEnd()}{reticences}{txtToAddInEnd}"; ;
        }
    }
}
