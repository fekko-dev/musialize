import { type AnyFieldApi, useForm } from "@tanstack/react-form";
import { useNavigate } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { Alert, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import type { components } from "@/lib/api-schema.gen";
import { useSuggestSong } from "../api/suggest-song";
import { songLimits } from "../song-limits";

const defaultValues = { title: "", artist: "", link: "" };

// openapi-react-query throws the response body; a 400's is a ValidationProblem.
function isValidationProblem(
  error: unknown,
): error is components["schemas"]["HttpValidationProblemDetails"] {
  return typeof error === "object" && error !== null && "errors" in error;
}

export function SuggestSongForm() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const suggestSong = useSuggestSong();

  const checkRequiredText = (value: string, max: number) => {
    if (value.trim() === "") return t("suggest.errors.required");
    if (value.length > max) return t("suggest.errors.tooLong", { max });
    return undefined;
  };

  // Same rule as the API's [AbsoluteHttpUrl]: an absolute http(s) URL.
  const checkLink = (value: string) => {
    const protocol = URL.parse(value.trim())?.protocol;
    return protocol === "http:" || protocol === "https:"
      ? undefined
      : t("suggest.errors.notALink");
  };

  const form = useForm({
    defaultValues,
    validators: {
      // Runs once the field checks pass. TanStack Form's validation guide maps a server's
      // field errors here; added to it (#44): the request itself is sent here too, so the
      // one call that saves the Song also gives the 400 to map.
      onSubmitAsync: async ({ value }) => {
        try {
          const song = await suggestSong.mutateAsync({ body: value });
          await navigate({
            to: "/songs/$songId",
            params: { songId: song.id },
          });
          return undefined;
        } catch (error) {
          const fields = isValidationProblem(error)
            ? Object.keys(error.errors ?? {})
                .filter((name) => name in defaultValues)
                .map((name) => [name, t("suggest.errors.invalid")])
            : [];
          return fields.length > 0
            ? { fields: Object.fromEntries(fields) }
            : t("suggest.failed");
        }
      },
    },
  });

  return (
    <form
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        void form.handleSubmit();
      }}
      className="flex flex-col gap-6"
    >
      <FieldGroup>
        <form.Field
          name="title"
          validators={{
            onBlur: ({ value }) => checkRequiredText(value, songLimits.title),
          }}
          children={(field) => (
            <TextField
              field={field}
              label={t("suggest.songTitle")}
              placeholder={t("suggest.songTitlePlaceholder")}
            />
          )}
        />
        <form.Field
          name="artist"
          validators={{
            onBlur: ({ value }) => checkRequiredText(value, songLimits.artist),
          }}
          children={(field) => (
            <TextField
              field={field}
              label={t("suggest.artist")}
              placeholder={t("suggest.artist")}
            />
          )}
        />
        <form.Field
          name="link"
          validators={{
            onBlur: ({ value }) =>
              checkRequiredText(value, songLimits.link) ?? checkLink(value),
          }}
          children={(field) => (
            <TextField
              field={field}
              label={t("suggest.link")}
              placeholder="https://…"
              description={t("suggest.linkHelp")}
              inputMode="url"
            />
          )}
        />
      </FieldGroup>
      <form.Subscribe
        selector={(state) => [state.errorMap.onSubmit, state.isSubmitting]}
        children={([formError, isSubmitting]) => (
          <>
            {typeof formError === "string" && (
              <Alert variant="destructive">
                <AlertTitle>{formError}</AlertTitle>
              </Alert>
            )}
            <Button type="submit" size="lg" disabled={!!isSubmitting}>
              {t("suggest.submit")}
            </Button>
          </>
        )}
      />
    </form>
  );
}

type TextFieldProps = {
  field: AnyFieldApi;
  label: string;
  placeholder: string;
  description?: string;
  inputMode?: "url";
};

// Wired as in shadcn's TanStack Form guide.
function TextField({
  field,
  label,
  placeholder,
  description,
  inputMode,
}: TextFieldProps) {
  const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid;
  const descriptionId = `${field.name}-description`;

  return (
    <Field data-invalid={isInvalid}>
      <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
      <Input
        id={field.name}
        name={field.name}
        value={field.state.value}
        onBlur={field.handleBlur}
        onChange={(event) => field.handleChange(event.target.value)}
        aria-invalid={isInvalid}
        aria-describedby={description ? descriptionId : undefined}
        placeholder={placeholder}
        inputMode={inputMode}
        autoComplete="off"
      />
      {description && (
        <FieldDescription id={descriptionId}>{description}</FieldDescription>
      )}
      {isInvalid && (
        // TanStack Form keeps the strings the validators return; FieldError takes { message }.
        <FieldError
          errors={field.state.meta.errors.map((message: string) => ({
            message,
          }))}
        />
      )}
    </Field>
  );
}
