import * as React from 'react';
import { Field } from 'redux-form';
import { ValueComponentProps } from './ValueComponent';
import { Label } from 'reactstrap';
import { capitalize } from '../../../../../helpers/string';
import * as classNames from 'classNames';

interface ValueBooleanComponentProps extends ValueComponentProps {

}

const id = (name: string, item) => `Value${capitalize(name)}${capitalize(item + "")}Component`

const ValueBooleanComponent = (props: ValueBooleanComponentProps) => <>
    {[true, false].map(item =>
    <Label key={id(props.name, item)} htmlFor={id(props.name, item)}>
            <Field
                key={id(props.name, item)}
                id={id(props.name, item)}
                type="radio"
                name={`${props.name}.valueBoolean`}
                component="input"
                value={item as any} // HACK: redux-form needs to get value as boolean as it is NO as string
                parse={value => value === "true"}
                onFocus={() => props.onSimulateValueClick(props.id)}
                onChange={() => props.onSimulateValueClick(props.id)}
                onClick={() => props.onSimulateValueClick(props.id)}
                disabled={props.disabled} />
            {item ? "Ano" : "Ne"}
        </Label>)}
    </>;

export default ValueBooleanComponent;