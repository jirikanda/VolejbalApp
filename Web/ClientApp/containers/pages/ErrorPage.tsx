import * as React from "react";
import { env } from 'process';
import { ApplicationError } from '../../models/ApplicationError';

interface ErrorPageProps {
    error?: Error;
    errorInfo?: React.ErrorInfo;
    applicationError?: ApplicationError;
}

const GenericErrorContent = () =>
    <>
    <h1>Chyba</h1>
    <p>Při zpracování Vašeho požadavku došlo bohužel k chybě.</p>
    <p>Pokud se Vám to stalo v této situaci poprvé, zkuste prosím svoji operaci zopakovat později.</p>
    <p>
        Veškeré chyby aplikace jsou zaznamenávány a odesílány k dalšímu zpracování.
        Pokud chcete ověřit, zdali je tento problém již evidován, či v jakém stavu řešení se nachází, neváhejte nás kontaktovat.
	    Veškeré zjištěné potíže se snažíme odstraňovat v nejbližším možném termínu, v prioritách dle závažnosti problému. Děkujeme za pochopení!
    </p>
    <p>Broker Trust, a.s.</p>
    </>


interface ApplicationErrorContentProps {
    message: string;
}
const ApplicationErrorContent = (props: ApplicationErrorContentProps) =>
    <>
        <h1>Chyba</h1>
        <p>{props.message}</p>
        <p>Broker Trust, a.s.</p>
    </>

const ErrorPage = (props: ErrorPageProps) =>
    <div className="container">
        {props.applicationError && props.applicationError.message
            ? <ApplicationErrorContent message={props.applicationError.message} />
            : <GenericErrorContent />
        }
    </div>

export default ErrorPage