import * as moment from 'moment';
import config from '../configuration/config';

export const required = value => (value != null && value.length > 0) ? null : "Vyplňte hodnotu";
export const maxLength = (maxLength: number) => (value: string) => ((value.length <= maxLength) ? null : `Hodnota musí být kratší než ${maxLength} nebo rovna.`);
export const minLength = (minLength: number) => (value: string) => ((value.length >= minLength) ? null : `Hodnota musí být delší než ${minLength} nebo rovna.`);
export const maxValue = (maxValue: number) => (value: number) => ((value <= maxValue) ? null : `Hodnota musí být menší než ${maxValue} nebo rovna.`);
export const minValue = (minValue: number) => (value: number) => ((value >= minValue) ? null : `Hodnota musí být větší než ${minValue} nebo rovna.`);
export const integer = value => value && !isNaN(parseInt(value)) ? null : 'Hodnota musí být celé číslo.';
export const float = value => value && !isNaN(parseFloat(value)) ? null : 'Hodnota musí být desetinné číslo.';

export const dateTime = (value) => moment(value).isValid() ? null : `Hodnota musí být validní datum.`;
export const maxDateTime = (value: Date) => moment(value).diff(moment(), "milliseconds") > 1 ? null : `Hodnota musí být větší než ${moment(value).format(config.dateFormat)} nebo rovna.`;
export const minDateTime = (value: Date) => moment(value).diff(moment(), "milliseconds") < 1 ? null : `Hodnota musí být menší než ${moment(value).format(config.dateFormat)} nebo rovna.`;
export const maxDate = (value: Date) => moment(value).diff(moment(), "days") > 1 ? null : `Hodnota musí být větší než ${moment(value).format(config.dateTimeFormat)} nebo rovna.`;
export const minDate = (value: Date) => moment(value).diff(moment(), "days") < 1 ? null : `Hodnota musí být menší než ${moment(value).format(config.dateTimeFormat)} nebo rovna.`;