using Projeto.Moope.Api.Validation;
using Projeto.Moope.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Api.Attributes
{
    public class DocumentoAttribute : ValidationAttribute
    {
        private readonly string _tipoPessoaCampo;

        public DocumentoAttribute(string tipoPessoaCampo)
        {
            _tipoPessoaCampo = tipoPessoaCampo;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string documento = Convert.ToString(value)?.Trim().Replace(".", "").Replace("-", "").Replace("/", "");

            var tipoPessoaProp = validationContext.ObjectType.GetProperty(_tipoPessoaCampo);
            if (tipoPessoaProp == null)
                return new ValidationResult($"Campo '{_tipoPessoaCampo}' não encontrado.");

            var tipoPessoa = (TipoPessoa)tipoPessoaProp.GetValue(validationContext.ObjectInstance);
            if (tipoPessoa == null)
                return new ValidationResult("Tipo de pessoa não informado.");

            var valido = tipoPessoa == TipoPessoa.FISICA
                ? Documentos.IsValidCpf(documento)
                : Documentos.IsValidCnpj(documento);

            return (valido)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? "Documento inválido.");
        }
    }
}
