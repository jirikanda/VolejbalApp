import * as React from 'react';
import { Field } from 'redux-form';
import { ValueComponentProps } from './ValueComponent';
import InputFormField from '../../../../form/InputFormField';
import ValidatedInputFormField from '../../../../form/ValidatedInputFormField';

interface ValueDateComponentProps extends ValueComponentProps {

}

const ValueDateComponent = (props: ValueDateComponentProps) =>
    <Field
        key="ValueDateTimeComponent"
        type="date"
        name={`${props.name}.valueDateTime`}
        component={ValidatedInputFormField}
        onFocus={(event) => props.onValueChange(event, props.id)}
        onChange={(event) => props.onValueChange(event, props.id)}
        onClick={(event) => props.onValueChange(event, props.id)}
        disabled={props.disabled}
        required />

export default ValueDateComponent;