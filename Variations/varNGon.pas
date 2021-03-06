{$Include 'delphiversion.pas'}

unit varNGon;

interface

uses
  Variation, VariationPoolManager;

type
  TVariationNGon = class(TVariation)
  private
    ngon_sides : integer;
    ngon_power, ngon_circle, ngon_corners : double;
    cpower, csides, csidesinv : double;
  public
    constructor Create;

    class function GetName: string; override;
    class function GetInstance: TVariation; override;

    function GetNrVariables: integer; override;
    function GetVariableNameAt(const Index: integer): string; override;

    function SetVariable(const Name: string; var value: double): boolean; override;
    function GetVariable(const Name: string; var value: double): boolean; override;
    function ResetVariable(const Name: string): boolean; override;

    procedure Prepare; override;
    procedure CalcFunction; override;
  end;

implementation

uses
  Math;

///////////////////////////////////////////////////////////////////////////////
procedure TVariationNGon.Prepare;
begin
  cpower := -0.5 * ngon_power;
  csides := 2.0 * PI / ngon_sides;
  csidesinv := 1.0 / csides;
end;
procedure TVariationNGon.CalcFunction;
var
  r_factor, theta, phi, amp: double;
begin

  if (FTX^ = 0) and (FTY^ = 0) then r_factor := 0
  else r_factor := Power(FTx^ * FTx^ + FTy^ * FTy^, cpower);

  theta := ArcTan2(FTy^, FTx^);

  phi := theta - csides * floor(theta * csidesinv);
  if (phi > 0.5 * csides) then
    phi := phi - csides;

  amp := (ngon_corners * (1.0 / cos(phi) - 1.0) + ngon_circle) * VVAR * r_factor;

  FPx^ := FPx^ + amp * FTx^;
  FPy^ := FPy^ + amp * FTy^;
{$ifndef Pre15c}
  FPz^ := FPz^ + vvar * FTz^;
{$endif}
end;

///////////////////////////////////////////////////////////////////////////////
constructor TVariationNGon.Create;
begin
  ngon_sides := 4;
  ngon_power := 2;
  ngon_circle := 1;
  ngon_corners := 1;
end;

///////////////////////////////////////////////////////////////////////////////
class function TVariationNGon.GetInstance: TVariation;
begin
  Result := TVariationNGon.Create;
end;

///////////////////////////////////////////////////////////////////////////////
class function TVariationNGon.GetName: string;
begin
  Result := 'ngon';
end;

///////////////////////////////////////////////////////////////////////////////
function TVariationNGon.GetVariableNameAt(const Index: integer): string;
begin
  case Index Of
  0: Result := 'ngon_sides';
  1: Result := 'ngon_power';
  2: Result := 'ngon_circle';
  3: Result := 'ngon_corners';
  else
    Result := '';
  end
end;

///////////////////////////////////////////////////////////////////////////////
function TVariationNGon.SetVariable(const Name: string; var value: double): boolean;
begin
  Result := False;
  if Name = 'ngon_sides' then begin
    if (value < 0) and (value > -1) then Value := -1
    else if (value >= 0) and (value < 1) then Value := 1;
    ngon_sides := Round(value);
    Result := True;
  end else if Name = 'ngon_power' then begin
    ngon_power := Value;
    Result := True;
  end else if Name = 'ngon_circle' then begin
    ngon_circle := Value;
    Result := True;
  end else if Name = 'ngon_corners' then begin
    ngon_corners := Value;
    Result := True;
  end;
end;
function TVariationNGon.ResetVariable(const Name: string): boolean;
begin
  Result := False;
  if Name = 'ngon_sides' then begin
    ngon_sides := 4;
    Result := True;
  end else if Name = 'ngon_power' then begin
    ngon_power := 2;
    Result := True;
  end else if Name = 'ngon_circle' then begin
    ngon_circle := 1;
    Result := True;
  end else if Name = 'ngon_corners' then begin
    ngon_corners := 1;
    Result := True;
  end;
end;

///////////////////////////////////////////////////////////////////////////////
function TVariationNGon.GetNrVariables: integer;
begin
  Result := 4
end;

///////////////////////////////////////////////////////////////////////////////
function TVariationNGon.GetVariable(const Name: string; var value: double): boolean;
begin
  Result := False;
  if Name = 'ngon_sides' then begin
    Value := ngon_sides;
    Result := True;
  end else if Name = 'ngon_power' then begin
    Value := ngon_power;
    Result := True;
  end else if Name = 'ngon_circle' then begin
    Value := ngon_circle;
    Result := True;
  end else if Name = 'ngon_corners' then begin
    Value := ngon_corners;
    Result := True;
  end;
end;

///////////////////////////////////////////////////////////////////////////////
initialization RegisterVariation(TIntegratedVariationLoader.Create(TVariationNgon, {$ifndef Pre15c}false{$else}true{$endif}, false)) end.
