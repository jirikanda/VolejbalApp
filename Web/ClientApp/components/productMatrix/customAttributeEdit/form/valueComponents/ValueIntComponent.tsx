import * as React from 'react';
import { Field } from 'redux-form';
import ValidatedInputFormField from '../../../../../components/form/ValidatedInputFormField';
import { ValueComponentProps } from './ValueComponent';
import InputFormField from '../../../../form/InputFormField';

interface ValueIntComponentProps extends ValueComponentProps {

}

const ValueIntComponent = (props: ValueIntComponentProps) =>
    <Field
        key="ValueIntComponent"
        type="number"
        name={`${props.name}.valueInt`}
        parse={value => value && parseInt(value)}
        component={ValidatedInputFormField}
        label={props.customAttribute.measureUnit}
        onFocus={(event) => props.onValueChange(event, props.id)}
        onChange={(event) => props.onValueChange(event, props.id)}
        onClick={(event) => props.onValueChange(event, props.id)}
        disabled={props.disabled}
        required />

export default ValueIntComponent;