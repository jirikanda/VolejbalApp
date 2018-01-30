import * as React from 'react';
import { Field } from 'redux-form';
import { ValueComponentProps } from './ValueComponent';
import { Label } from 'reactstrap';
import { capitalize } from '../../../../../helpers/string';
import * as classNames from 'classNames';
import { BooleanExtended } from '../../../../../apimodels/CustomAttributeValueStore';

interface ValueBooleanExtendedComponentProps extends ValueComponentProps {

}

const id = (name: string, item) => `Value${capitalize(name)}${capitalize(item + "")}Component`

const ValueBooleanExtendedComponent = (props: ValueBooleanExtendedComponentProps) => <>
    {[BooleanExtended.True, BooleanExtended.Limited, BooleanExtended.False].map(item =>
        <Label key={id(props.name, item)} htmlFor={id(props.name, item)} className="mr-2">
            <Field
                key={id(props.name, item)}
                id={id(props.name, item)}
                type="radio"
                name={`${props.name}.valueBooleanExtended`}
                value={item}
                parse={(value) => parseInt(value)}
                onFocus={() => props.onSimulateValueClick(props.id)}
                onChange={() => props.onSimulateValueClick(props.id)}
                onClick={() => props.onSimulateValueClick(props.id)}
                component="input"
                disabled={props.disabled} />
            {item === BooleanExtended.True && "Ano"}
            {item === BooleanExtended.Limited && "Omezeně"}
            {item === BooleanExtended.False && "Ne"}
        </Label>
    )}</>

export default ValueBooleanExtendedComponent;