import * as React from 'react';
import { Field } from 'redux-form'

export interface ValueRadioButtonProps {
    id: string;
    name: string;
    onChange: (event, id: string) => void;
    disabled?: boolean;
    value?: string | number;
}

const ValueRadioButton = (props: ValueRadioButtonProps) =>
    <label>
        <Field
            name={props.name}
            id={props.id}
            type="radio"
            component="input"
            value={props.value}
            parse={(value) => parseInt(value)}
            disabled={props.disabled}
            onChange={(event) => props.onChange(event, props.id)}
        />
    </label>

export default ValueRadioButton