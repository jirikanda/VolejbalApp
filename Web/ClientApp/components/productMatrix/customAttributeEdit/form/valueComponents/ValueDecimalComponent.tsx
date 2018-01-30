import * as React from 'react';
import { Field } from 'redux-form';
import ValidatedInputFormField from '../../../../form/ValidatedInputFormField';
import { ValueComponentProps } from './ValueComponent';
import InputFormField from '../../../../form/InputFormField';

interface ValueDecimalComponentProps extends ValueComponentProps {
    step: number;
}

const ValueDecimalComponent = (props: ValueDecimalComponentProps) => <Field
    key="ValueDecimalComponent"
    type="number"
    step={props.step}
    name={`${props.name}.valueDecimal`}
    parse={value => parseFloat(value)}
    label={props.customAttribute.measureUnit}
    component={ValidatedInputFormField}
    onFocus={(event) => props.onValueChange(event, props.id)}
    onChange={(event) => props.onValueChange(event, props.id)}
    onClick={(event) => props.onValueChange(event, props.id)}
    disabled={props.disabled}
    required />

export default ValueDecimalComponent;