using System.Text.RegularExpressions;

namespace Projeto.Moope.Cliente.Core.Validation
{
    public static class Documentos
    {
        public static string OnlyDigits(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

        public static bool IsValidCpf(string? value)
        {
            var cpf = OnlyDigits(value);
            if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11 || Regex.IsMatch(cpf, @"^(\d)\1+$"))
                return false;

            var soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (cpf[i] - '0') * (10 - i);

            var resto = (soma * 10) % 11;
            if (resto == 10) resto = 0;
            if (resto != (cpf[9] - '0')) return false;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (cpf[i] - '0') * (11 - i);

            resto = (soma * 10) % 11;
            if (resto == 10) resto = 0;

            return resto == (cpf[10] - '0');
        }

        public static bool IsValidCnpj(string? value)
        {
            var cnpj = OnlyDigits(value);
            if (string.IsNullOrWhiteSpace(cnpj) || cnpj.Length != 14 || Regex.IsMatch(cnpj, @"^(\d)\1+$"))
                return false;

            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var temp = cnpj.Substring(0, 12);
            var soma = 0;

            for (int i = 0; i < 12; i++)
                soma += (temp[i] - '0') * multiplicador1[i];

            var resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            temp += resto;
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += (temp[i] - '0') * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            return cnpj.EndsWith(resto.ToString(), StringComparison.Ordinal);
        }
    }
}
